using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PayNexa.Common.Initialization;
using PayNexa.Common.Logging;

namespace PayNexa.MongoDb.Indexes;

internal sealed partial class MongoIndexInitializer(
    IMongoDatabase database,
    IEnumerable<IMongoIndexDefinition> definitions,
    ILogger<MongoIndexInitializer> logger)
    : IInfrastructureInitializer
{
    private const int MaxAttempts = 12;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public int Order => InitializationOrder.ReadStore;

    public string Name => $"MongoDB {database.DatabaseNamespace.DatabaseName} collections and indexes";

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        foreach (var definition in definitions)
        {
            await EnsureWithRetryAsync(definition, cancellationToken);
        }
    }

    private async Task EnsureWithRetryAsync(IMongoIndexDefinition definition, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            using var step = logger.BeginStep($"Ensure collection and indexes on {definition.CollectionName}");

            try
            {
                await definition.EnsureIndexesAsync(database, cancellationToken);
                step.Succeeded();
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts && exception is not OperationCanceledException)
            {
                step.Failed(exception);
                LogRetry(logger, definition.CollectionName, attempt, MaxAttempts, RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }

    [LoggerMessage(EventId = LogEventIds.MongoDb + 10, Level = LogLevel.Warning,
        Message = "MongoDB not ready for {Collection} (attempt {Attempt}/{MaxAttempts}); retrying in {RetryDelaySeconds} s")]
    private static partial void LogRetry(ILogger logger, string collection, int attempt, int maxAttempts, double retryDelaySeconds);
}
