using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PayNexa.MongoDb;

internal sealed class MongoDbHealthCheck(IMongoDatabase database) : IHealthCheck
{
    public const string UnhealthyDescription = "MongoDB is unreachable.";

    private static readonly BsonDocument Ping = new("ping", 1);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await database.RunCommandAsync<BsonDocument>(Ping, cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy(UnhealthyDescription, exception);
        }
    }
}
