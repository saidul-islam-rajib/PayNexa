using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PayNexa.Common.Logging;

namespace PayNexa.MongoDb.Indexes;

internal sealed partial class MongoIndexInitializer(
    IMongoDatabase database,
    IEnumerable<IMongoIndexDefinition> definitions,
    ILogger<MongoIndexInitializer> logger)
    : BackgroundService
{
    private const int MaxAttempts = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var operation = OperationContext.Begin("MongoIndexInitialization");

        foreach (var definition in definitions)
        {
            await EnsureWithRetryAsync(definition, stoppingToken);
        }
    }

    private async Task EnsureWithRetryAsync(IMongoIndexDefinition definition, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts && !cancellationToken.IsCancellationRequested; attempt++)
        {
            using var step = logger.BeginStep($"Ensure indexes on {definition.CollectionName}");

            try
            {
                await definition.EnsureIndexesAsync(database, cancellationToken);
                step.Succeeded();
                return;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                step.Failed(exception, attempt == MaxAttempts ? LogLevel.Error : LogLevel.Warning);
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, attempt))), cancellationToken);
        }

        LogGaveUp(logger, definition.CollectionName, MaxAttempts);
    }

    [LoggerMessage(EventId = LogEventIds.MongoDb + 10, Level = LogLevel.Error,
        Message = "Gave up ensuring indexes on {Collection} after {Attempts} attempts; queries may be slow until the service restarts")]
    private static partial void LogGaveUp(ILogger logger, string collection, int attempts);
}
