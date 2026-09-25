using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PayNexa.Common.Caching;
using PayNexa.Common.HealthChecks;
using StackExchange.Redis;

namespace PayNexa.Caching;

public static class CachingExtensions
{
    public static IHostApplicationBuilder AddPayNexaRedisCache(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddOptions<RedisOptions>()
            .BindConfiguration(RedisOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<RedisOptions>>().Value;
            var configuration = ConfigurationOptions.Parse(options.ConnectionString);
            configuration.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configuration);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddHealthChecks().AddCheck<RedisHealthCheck>("redis", tags: [HealthCheckTags.Ready]);

        return builder;
    }
}
