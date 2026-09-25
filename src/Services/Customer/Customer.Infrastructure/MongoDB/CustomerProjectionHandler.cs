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
        ProjectAsync(new CustomerReadModel
        {
            Id = message.CustomerId,
            FirstName = message.FirstName,
            LastName = message.LastName,
            Email = message.Email,
            PhoneNumber = message.PhoneNumber,
            DateOfBirth = message.DateOfBirth,
            Status = message.Status,
            CreatedAtUtc = message.CreatedAtUtc,
            UpdatedAtUtc = message.UpdatedAtUtc,
            Version = message.Version,
        }, cancellationToken);

    public Task HandleAsync(CustomerUpdatedIntegrationEvent message, CancellationToken cancellationToken) =>
        ProjectAsync(new CustomerReadModel
        {
            Id = message.CustomerId,
            FirstName = message.FirstName,
            LastName = message.LastName,
            Email = message.Email,
            PhoneNumber = message.PhoneNumber,
            DateOfBirth = message.DateOfBirth,
            Status = message.Status,
            CreatedAtUtc = message.CreatedAtUtc,
            UpdatedAtUtc = message.UpdatedAtUtc,
            Version = message.Version,
        }, cancellationToken);

    private async Task ProjectAsync(CustomerReadModel readModel, CancellationToken cancellationToken)
    {
        using (var step = logger.BeginStep("Project customer to read store")
                   .WithProperty("CustomerId", readModel.Id)
                   .WithProperty("CustomerVersion", readModel.Version))
        {
            var applied = await customers.UpsertIfNewerAsync(readModel, cancellationToken);
            step.WithProperty("ProjectionApplied", applied).Succeeded();
        }

        await cache.RemoveAsync(CustomerCacheKeys.Profile(readModel.Id), cancellationToken);
    }
}
