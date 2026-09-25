namespace PayNexa.SharedKernel.Domain;

public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents, IAuditable, IVersioned
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }

    public DateTime CreatedAtUtc { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; private set; }

    public string UpdatedBy { get; private set; } = string.Empty;

    public long Version { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public IReadOnlyList<IDomainEvent> DequeueDomainEvents()
    {
        var dequeued = _domainEvents.ToArray();
        _domainEvents.Clear();
        return dequeued;
    }

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
