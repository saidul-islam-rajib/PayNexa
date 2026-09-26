using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PayNexa.Messaging.Kafka;

internal sealed class KafkaHealthCheck(IAdminClient adminClient) : IHealthCheck
{
    public const string DegradedDescription = "Kafka is unreachable; integration events stay in the outbox until it recovers.";

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(2));
            return Task.FromResult(HealthCheckResult.Healthy($"{metadata.Brokers.Count} broker(s)"));
        }
        catch (KafkaException exception)
        {
            return Task.FromResult(HealthCheckResult.Degraded(DegradedDescription, exception));
        }
    }
}
