using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using PayNexa.Caching;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Contracts.Events;
using PayNexa.Customers.Infrastructure.MongoDB;
using PayNexa.Customers.Infrastructure.Persistence;
using PayNexa.Customers.Infrastructure.Repositories;
using PayNexa.MongoDb;
using PayNexa.SqlServer;

namespace PayNexa.Customers.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddCustomerInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddPayNexaSqlServer<CustomerDbContext>(CustomerDbContext.ConnectionStringName, CustomerDbContext.Schema);
        builder.AddPayNexaMongoDb();
        builder.AddPayNexaRedisCache();

        var services = builder.Services;

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        services.AddSingleton(provider => provider.GetRequiredService<IMongoDatabase>()
            .GetCollection<CustomerReadModel>(CustomerReadModel.CollectionName));
        services.AddScoped<ICustomerReadStore, CustomerReadStore>();
        services.AddMongoIndexes<CustomerReadModelIndexes>();

        services.AddOutboxHandler<CustomerCreatedIntegrationEvent, CustomerProjectionHandler>();
        services.AddOutboxHandler<CustomerUpdatedIntegrationEvent, CustomerProjectionHandler>();

        return builder;
    }

    public static Task ApplyCustomerDatabaseMigrationsAsync(this IHost host, CancellationToken cancellationToken = default) =>
        host.ApplyDatabaseMigrationsAsync<CustomerDbContext>(cancellationToken);
}
