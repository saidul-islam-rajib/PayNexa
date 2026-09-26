using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using PayNexa.Common.HealthChecks;
using PayNexa.Common.Initialization;
using PayNexa.MongoDb.Indexes;
using PayNexa.MongoDb.Logging;

namespace PayNexa.MongoDb;

public static class MongoDbExtensions
{
    public static IServiceCollection AddPayNexaMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        MongoConventions.EnsureRegistered();

        services.AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(MongoDbOptions.SectionName))
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

        services.AddSingleton<IInfrastructureInitializer, MongoIndexInitializer>();
        services.AddHealthChecks().AddCheck<MongoDbHealthCheck>("mongodb", tags: [HealthCheckTags.Ready]);

        return services;
    }

    public static IServiceCollection AddMongoCollection<TDocument>(this IServiceCollection services, string collectionName) =>
        services.AddSingleton(provider => provider.GetRequiredService<IMongoDatabase>().GetCollection<TDocument>(collectionName));

    public static IServiceCollection AddMongoIndexes<TDefinition>(this IServiceCollection services)
        where TDefinition : class, IMongoIndexDefinition =>
        services.AddSingleton<IMongoIndexDefinition, TDefinition>();
}
