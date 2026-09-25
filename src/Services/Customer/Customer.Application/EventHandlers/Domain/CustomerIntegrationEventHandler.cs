using PayNexa.Common.DomainEvents;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application.Mappings;
using PayNexa.Customers.Contracts.Events;
using PayNexa.Customers.Domain.CustomerAggregate.Events;

namespace PayNexa.Customers.Application.EventHandlers.Domain;

public sealed class CustomerIntegrationEventHandler(IOutbox outbox) :
    IDomainEventHandler<CustomerRegisteredDomainEvent>,
    IDomainEventHandler<CustomerProfileUpdatedDomainEvent>,
    IDomainEventHandler<CustomerEmailChangedDomainEvent>,
    IDomainEventHandler<CustomerAddressChangedDomainEvent>,
    IDomainEventHandler<CustomerSuspendedDomainEvent>,
    IDomainEventHandler<CustomerReactivatedDomainEvent>,
    IDomainEventHandler<CustomerClosedDomainEvent>,
    IDomainEventHandler<CustomerKycVerifiedDomainEvent>,
    IDomainEventHandler<CustomerKycRejectedDomainEvent>
{
    public Task HandleAsync(CustomerRegisteredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        outbox.Enqueue(new CustomerCreatedIntegrationEvent(domainEvent.EventId, domainEvent.OccurredAtUtc, domainEvent.Customer.ToSnapshot()));
        return Task.CompletedTask;
    }

    public Task HandleAsync(CustomerProfileUpdatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        EnqueueUpdated(domainEvent, CustomerChangeTypes.ProfileUpdated);

    public Task HandleAsync(CustomerEmailChangedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        EnqueueUpdated(domainEvent, CustomerChangeTypes.EmailChanged);

    public Task HandleAsync(CustomerAddressChangedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        EnqueueUpdated(domainEvent, CustomerChangeTypes.AddressChanged);

    public Task HandleAsync(CustomerSuspendedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        EnqueueUpdated(domainEvent, CustomerChangeTypes.Suspended);

    public Task HandleAsync(CustomerReactivatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        EnqueueUpdated(domainEvent, CustomerChangeTypes.Reactivated);

    public Task HandleAsync(CustomerClosedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        EnqueueUpdated(domainEvent, CustomerChangeTypes.Closed);

    public Task HandleAsync(CustomerKycVerifiedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        EnqueueUpdated(domainEvent, CustomerChangeTypes.KycVerified);

    public Task HandleAsync(CustomerKycRejectedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        EnqueueUpdated(domainEvent, CustomerChangeTypes.KycRejected);

    private Task EnqueueUpdated(CustomerDomainEvent domainEvent, string changeType)
    {
        outbox.Enqueue(new CustomerUpdatedIntegrationEvent(domainEvent.EventId, domainEvent.OccurredAtUtc, changeType, domainEvent.Customer.ToSnapshot()));
        return Task.CompletedTask;
    }
}
