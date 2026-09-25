using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayNexa.Caching;
using PayNexa.Common.Initialization;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Contracts.Events;
using PayNexa.Customers.Infrastructure.MongoDB;
using PayNexa.Customers.Infrastructure.Persistence;
using PayNexa.Customers.Infrastructure.Persistence.Repositories;
using PayNexa.Customers.Infrastructure.Persistence.Seed;
using PayNexa.Messaging;
using PayNexa.MongoDb;
using PayNexa.SqlServer;

namespace PayNexa.Customers.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddWriteStore(configuration)
            .AddReadStore(configuration)
            .AddCaching(configuration)
            .AddMessaging(configuration);

    private static IServiceCollection AddWriteStore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPayNexaSqlServer<CustomerDbContext>(configuration, CustomerDbContext.ConnectionStringName, CustomerDbContext.Schema);
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IDataSeeder, CustomerDataSeeder>();

        return services;
    }

    private static IServiceCollection AddReadStore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPayNexaMongoDb(configuration);
        services.AddMongoCollection<CustomerReadModel>(CustomerReadModel.CollectionName);
        services.AddMongoIndexes<CustomerReadModelIndexes>();
        services.AddScoped<ICustomerReadStore, CustomerReadStore>();

        services.AddOutboxHandler<CustomerCreatedIntegrationEvent, CustomerProjectionHandler>();
        services.AddOutboxHandler<CustomerUpdatedIntegrationEvent, CustomerProjectionHandler>();

        return services;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration) =>
        services.AddPayNexaRedisCache(configuration);

    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddPayNexaKafka(configuration)
            .AddIntegrationEventPublishing<CustomerCreatedIntegrationEvent>()
            .AddIntegrationEventPublishing<CustomerUpdatedIntegrationEvent>();
}
