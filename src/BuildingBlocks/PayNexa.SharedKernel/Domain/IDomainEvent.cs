namespace PayNexa.SharedKernel.Domain;

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTime OccurredAtUtc { get; }
}
