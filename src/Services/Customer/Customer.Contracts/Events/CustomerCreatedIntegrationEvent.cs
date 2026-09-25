using PayNexa.Messaging.Abstractions;

namespace PayNexa.Customers.Contracts.Events;

[IntegrationEvent(CustomerEventTypes.Created, version: 1)]
public sealed record CustomerCreatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    CustomerSnapshot Customer) : IIntegrationEvent
{
    public string PartitionKey => Customer.Id.ToString();
}
