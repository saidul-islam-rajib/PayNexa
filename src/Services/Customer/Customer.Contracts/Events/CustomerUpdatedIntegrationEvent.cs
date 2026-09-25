using PayNexa.Messaging.Abstractions;

namespace PayNexa.Customers.Contracts.Events;

[IntegrationEvent(CustomerEventTypes.Updated, version: 1)]
public sealed record CustomerUpdatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    string ChangeType,
    CustomerSnapshot Customer) : IIntegrationEvent
{
    public string PartitionKey => Customer.Id.ToString();
}
