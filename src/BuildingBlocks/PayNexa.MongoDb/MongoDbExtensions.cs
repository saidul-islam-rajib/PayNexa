using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using PayNexa.Common.HealthChecks;
using PayNexa.MongoDb.Indexes;
using PayNexa.MongoDb.Logging;

namespace PayNexa.MongoDb;

public static class MongoDbExtensions
{
    public static IHostApplicationBuilder AddPayNexaMongoDb(this IHostApplicationBuilder builder)
    {
        MongoConventions.EnsureRegistered();

        var services = builder.Services;

        services.AddOptions<MongoDbOptions>()
            .BindConfiguration(MongoDbOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<MongoCommandLogger>();
        services.AddSingleton<IMongoClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            var commandLogger = provider.GetRequiredService<MongoCommandLogger>();
            var settings = MongoClientSettings.FromConnectionString(options.ConnectionString);

            settings.ClusterConfigurator = cluster => cluster
                .Subscribe<CommandStartedEvent>(commandLogger.OnStarted)
                .Subscribe<CommandSucceededEvent>(commandLogger.OnSucceeded)
                .Subscribe<CommandFailedEvent>(commandLogger.OnFailed);

            return new MongoClient(settings);
        });

        services.AddSingleton(provider => provider.GetRequiredService<IMongoClient>()
            .GetDatabase(provider.GetRequiredService<IOptions<MongoDbOptions>>().Value.DatabaseName));

        services.AddHostedService<MongoIndexInitializer>();
        services.AddHealthChecks().AddCheck<MongoDbHealthCheck>("mongodb", tags: [HealthCheckTags.Ready]);

        return builder;
    }

    public static IServiceCollection AddMongoIndexes<TDefinition>(this IServiceCollection services)
        where TDefinition : class, IMongoIndexDefinition =>
        services.AddSingleton<IMongoIndexDefinition, TDefinition>();
}
