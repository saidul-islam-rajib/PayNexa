namespace PayNexa.SharedKernel.Domain;

public abstract record DomainEvent(DateTime OccurredAtUtc) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();
}
