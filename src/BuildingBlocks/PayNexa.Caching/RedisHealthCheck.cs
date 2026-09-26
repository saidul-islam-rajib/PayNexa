using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace PayNexa.Caching;

internal sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public const string DegradedDescription = "Redis is unreachable; reads fall back to the read store.";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await redis.GetDatabase().PingAsync().WaitAsync(cancellationToken);
            return HealthCheckResult.Healthy($"Ping {latency.TotalMilliseconds:0.##} ms");
        }
        catch (Exception exception) when (exception is RedisException or TimeoutException)
        {
            return HealthCheckResult.Degraded(DegradedDescription, exception);
        }
    }
}
