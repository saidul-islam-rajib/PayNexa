namespace PayNexa.Messaging.Abstractions;

public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTime OccurredAtUtc { get; }

    string PartitionKey { get; }
}
