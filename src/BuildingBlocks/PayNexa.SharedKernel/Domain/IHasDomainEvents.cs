namespace PayNexa.SharedKernel.Domain;

public interface IHasDomainEvents
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    IReadOnlyList<IDomainEvent> DequeueDomainEvents();
}
