using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PayNexa.Common.Caching;
using PayNexa.Common.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Caching;
using PayNexa.Customers.Contracts.Events;
using PayNexa.MongoDb.ReadModels;

namespace PayNexa.Customers.Infrastructure.MongoDB;

internal sealed class CustomerProjectionHandler(
    IMongoCollection<CustomerReadModel> customers,
    ICacheService cache,
    ILogger<CustomerProjectionHandler> logger)
    : IOutboxMessageHandler<CustomerCreatedIntegrationEvent>,
      IOutboxMessageHandler<CustomerUpdatedIntegrationEvent>
{
    public Task HandleAsync(CustomerCreatedIntegrationEvent message, CancellationToken cancellationToken) =>
        ProjectAsync(message.Customer, cancellationToken);

    public Task HandleAsync(CustomerUpdatedIntegrationEvent message, CancellationToken cancellationToken) =>
        ProjectAsync(message.Customer, cancellationToken);

    private async Task ProjectAsync(CustomerSnapshot snapshot, CancellationToken cancellationToken)
    {
        using (var step = logger.BeginStep("Project customer to read store")
                   .WithProperty("CustomerId", snapshot.Id)
                   .WithProperty("CustomerVersion", snapshot.Version))
        {
            var applied = await customers.UpsertIfNewerAsync(snapshot.ToReadModel(), cancellationToken);
            step.WithProperty("ProjectionApplied", applied).Succeeded();
        }

        await cache.RemoveAsync(CustomerCacheKeys.Profile(snapshot.Id), cancellationToken);
    }
}
