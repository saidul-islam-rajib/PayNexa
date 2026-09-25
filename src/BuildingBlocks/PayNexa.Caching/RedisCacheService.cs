using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Caching;
using PayNexa.Common.Logging;
using StackExchange.Redis;

namespace PayNexa.Caching;

internal sealed class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        using var step = logger.BeginStep("Redis GET").WithProperty("CacheKey", key);

        try
        {
            var value = await redis.GetDatabase().StringGetAsync(key).WaitAsync(cancellationToken);

            if (value.IsNullOrEmpty)
            {
                step.WithProperty("CacheHit", false).Succeeded();
                return null;
            }

            var cached = JsonSerializer.Deserialize<T>(value.ToString(), JsonOptions);
            step.WithProperty("CacheHit", cached is not null).Succeeded();
            return cached;
        }
        catch (JsonException exception)
        {
            step.Failed(exception);
            await RemoveUnreadableEntryAsync(key);
            return null;
        }
        catch (Exception exception) when (exception is RedisException or TimeoutException)
        {
            step.WithProperty("CacheHit", false).Failed(exception);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan timeToLive, CancellationToken cancellationToken = default)
        where T : class
    {
        using var step = logger.BeginStep("Redis SET").WithProperty("CacheKey", key).WithProperty("TtlSeconds", timeToLive.TotalSeconds);

        try
        {
            var payload = JsonSerializer.Serialize(value, JsonOptions);
            await redis.GetDatabase().StringSetAsync(key, payload, timeToLive).WaitAsync(cancellationToken);
            step.Succeeded();
        }
        catch (Exception exception) when (exception is RedisException or TimeoutException)
        {
            step.Failed(exception);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        using var step = logger.BeginStep("Redis DEL").WithProperty("CacheKey", key);

        try
        {
            await redis.GetDatabase().KeyDeleteAsync(key).WaitAsync(cancellationToken);
            step.Succeeded();
        }
        catch (Exception exception) when (exception is RedisException or TimeoutException)
        {
            step.Failed(exception);
        }
    }

    private async Task RemoveUnreadableEntryAsync(string key)
    {
        using var step = logger.BeginStep("Redis DEL unreadable entry").WithProperty("CacheKey", key);

        try
        {
            await redis.GetDatabase().KeyDeleteAsync(key);
            step.Succeeded();
        }
        catch (Exception exception) when (exception is RedisException or TimeoutException)
        {
            step.Failed(exception);
        }
    }
}
