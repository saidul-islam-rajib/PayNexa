using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PayNexa.Common.Caching;
using PayNexa.Common.HealthChecks;
using StackExchange.Redis;

namespace PayNexa.Caching;

public static class CachingExtensions
{
    public static IServiceCollection AddPayNexaRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<RedisOptions>>().Value;
            var redisConfiguration = ConfigurationOptions.Parse(options.ConnectionString);
            redisConfiguration.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(redisConfiguration);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis", tags: [HealthCheckTags.Ready]);

        return services;
    }
}
