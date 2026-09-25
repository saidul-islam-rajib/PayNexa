using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayNexa.Common.Correlation;
using PayNexa.Common.Logging;
using PayNexa.SqlServer.Logging;

namespace PayNexa.SqlServer.Outbox;

public sealed partial class OutboxProcessor<TDbContext>(
    IServiceScopeFactory scopeFactory,
    OutboxHandlerRegistry registry,
    IOptions<OutboxOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxProcessor<TDbContext>> logger)
    : BackgroundService
    where TDbContext : DbContext
{
    public const string ActivitySourceName = "PayNexa.Outbox";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private readonly OutboxOptions _options = options.Value;
    private bool _isFailing;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogProcessorStarted(logger, typeof(TDbContext).Name, _options.PollingInterval.TotalMilliseconds, _options.BatchSize);
        using var timer = new PeriodicTimer(_options.PollingInterval, timeProvider);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DrainAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }

        LogProcessorStopped(logger, typeof(TDbContext).Name);
    }

    private async Task DrainAsync(CancellationToken cancellationToken)
    {
        try
        {
            int processed;
            do
            {
                processed = await ProcessBatchAsync(cancellationToken);
            }
            while (processed == _options.BatchSize && !cancellationToken.IsCancellationRequested);

            if (_isFailing)
            {
                _isFailing = false;
                LogProcessingRecovered(logger, typeof(TDbContext).Name);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            if (!_isFailing)
            {
                _isFailing = true;
                LogBatchFailed(logger, exception, typeof(TDbContext).Name);
            }
        }
    }

    private async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var claimSql = BuildClaimSql(dbContext);
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            dbContext.ChangeTracker.Clear();
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            var now = timeProvider.GetUtcNow().UtcDateTime;
            var messages = await dbContext.Set<OutboxMessage>()
                .FromSqlRaw(claimSql, _options.BatchSize, _options.MaxAttempts, now)
                .TagWith(QueryTags.BackgroundPolling)
                .ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                await ProcessMessageAsync(scope.ServiceProvider, message, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return messages.Count;
        });
    }

    private async Task ProcessMessageAsync(IServiceProvider services, OutboxMessage message, CancellationToken cancellationToken)
    {
        var messageName = OutboxSerializer.ShortName(message.Type);

        using var correlation = message.CorrelationId is null ? null : CorrelationContext.Begin(message.CorrelationId);
        using var operation = OperationContext.Begin($"Outbox.{messageName}");
        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            [CorrelationContext.LogPropertyName] = message.CorrelationId,
            ["OutboxMessageId"] = message.Id,
            ["OutboxMessageType"] = messageName,
            ["OutboxAttempt"] = message.Attempts + 1,
        });
        using var activity = ActivitySource.StartActivity($"outbox process {messageName}", ActivityKind.Consumer, message.TraceParent);
        using var step = logger.BeginStep($"Dispatch {messageName}");

        if (!registry.TryGetDispatcher(message.Type, out var dispatch))
        {
            message.Attempts = _options.MaxAttempts;
            message.LastError = "No outbox handler is registered for this message type.";
            step.Failed(message.LastError);
            LogNoHandler(logger, messageName, message.Id);
            return;
        }

        try
        {
            await dispatch(services, message.Payload, cancellationToken);
            message.ProcessedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
            message.LastError = null;
            step.Succeeded();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            message.Attempts++;
            message.LastError = Truncate($"{exception.GetType().Name}: {exception.Message}", OutboxMessage.LastErrorMaxLength);

            if (message.Attempts >= _options.MaxAttempts)
            {
                step.Failed(exception, LogLevel.Error);
                LogDeadLettered(logger, messageName, message.Id, message.Attempts);
                return;
            }

            var delay = RetryDelay(message.Attempts);
            message.NextAttemptAtUtc = timeProvider.GetUtcNow().UtcDateTime.Add(delay);
            step.Failed(exception);
            LogRetryScheduled(logger, messageName, message.Id, message.Attempts, delay.TotalSeconds);
        }
    }

    private TimeSpan RetryDelay(int attempts)
    {
        var exponential = TimeSpan.FromSeconds(Math.Pow(2, Math.Min(attempts, 16)));
        return exponential < _options.MaxRetryDelay ? exponential : _options.MaxRetryDelay;
    }

    private static string BuildClaimSql(DbContext dbContext)
    {
        var entity = dbContext.Model.FindEntityType(typeof(OutboxMessage))
                     ?? throw new InvalidOperationException($"{dbContext.GetType().Name} does not map the outbox. Call modelBuilder.ApplyOutbox().");

        var table = entity.GetSchema() is { } schema
            ? $"[{schema}].[{entity.GetTableName()}]"
            : $"[{entity.GetTableName()}]";

        return $$"""
                SELECT TOP ({0}) *
                FROM {{table}} WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE [{{nameof(OutboxMessage.ProcessedAtUtc)}}] IS NULL
                  AND [{{nameof(OutboxMessage.Attempts)}}] < {1}
                  AND ([{{nameof(OutboxMessage.NextAttemptAtUtc)}}] IS NULL OR [{{nameof(OutboxMessage.NextAttemptAtUtc)}}] <= {2})
                ORDER BY [{{nameof(OutboxMessage.OccurredAtUtc)}}]
                """;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    [LoggerMessage(EventId = LogEventIds.SqlServer + 20, Level = LogLevel.Information,
        Message = "Outbox processor for {DbContext} started (every {PollingIntervalMs} ms, batch size {BatchSize})")]
    private static partial void LogProcessorStarted(ILogger logger, string dbContext, double pollingIntervalMs, int batchSize);

    [LoggerMessage(EventId = LogEventIds.SqlServer + 21, Level = LogLevel.Information,
        Message = "Outbox processor for {DbContext} stopped")]
    private static partial void LogProcessorStopped(ILogger logger, string dbContext);

    [LoggerMessage(EventId = LogEventIds.SqlServer + 22, Level = LogLevel.Error,
        Message = "Outbox processing for {DbContext} failed; retrying on the next poll until the database is reachable")]
    private static partial void LogBatchFailed(ILogger logger, Exception exception, string dbContext);

    [LoggerMessage(EventId = LogEventIds.SqlServer + 23, Level = LogLevel.Information,
        Message = "Outbox processing for {DbContext} recovered")]
    private static partial void LogProcessingRecovered(ILogger logger, string dbContext);

    [LoggerMessage(EventId = LogEventIds.SqlServer + 24, Level = LogLevel.Warning,
        Message = "Outbox message {OutboxMessageType} {OutboxMessageId} failed on attempt {Attempts}; retrying in {RetryDelaySeconds} s")]
    private static partial void LogRetryScheduled(ILogger logger, string outboxMessageType, Guid outboxMessageId, int attempts, double retryDelaySeconds);

    [LoggerMessage(EventId = LogEventIds.SqlServer + 25, Level = LogLevel.Critical,
        Message = "Outbox message {OutboxMessageType} {OutboxMessageId} dead-lettered after {Attempts} attempts; it needs manual attention")]
    private static partial void LogDeadLettered(ILogger logger, string outboxMessageType, Guid outboxMessageId, int attempts);

    [LoggerMessage(EventId = LogEventIds.SqlServer + 26, Level = LogLevel.Critical,
        Message = "No outbox handler registered for {OutboxMessageType}; message {OutboxMessageId} dead-lettered")]
    private static partial void LogNoHandler(ILogger logger, string outboxMessageType, Guid outboxMessageId);
}
