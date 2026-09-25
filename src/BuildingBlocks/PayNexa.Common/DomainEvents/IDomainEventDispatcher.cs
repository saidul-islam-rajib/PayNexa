using PayNexa.SharedKernel.Domain;

namespace PayNexa.Common.DomainEvents;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
