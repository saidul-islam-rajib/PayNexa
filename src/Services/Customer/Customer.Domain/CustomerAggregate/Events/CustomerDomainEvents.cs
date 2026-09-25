using PayNexa.SharedKernel.Domain;

namespace PayNexa.Customers.Domain.CustomerAggregate.Events;

public abstract record CustomerDomainEvent(Customer Customer, DateTime OccurredAtUtc) : DomainEvent(OccurredAtUtc);

public sealed record CustomerRegisteredDomainEvent(Customer Customer, DateTime OccurredAtUtc) : CustomerDomainEvent(Customer, OccurredAtUtc);

public sealed record CustomerProfileUpdatedDomainEvent(Customer Customer, DateTime OccurredAtUtc) : CustomerDomainEvent(Customer, OccurredAtUtc);

public sealed record CustomerEmailChangedDomainEvent(Customer Customer, DateTime OccurredAtUtc) : CustomerDomainEvent(Customer, OccurredAtUtc);

public sealed record CustomerAddressChangedDomainEvent(Customer Customer, DateTime OccurredAtUtc) : CustomerDomainEvent(Customer, OccurredAtUtc);

public sealed record CustomerSuspendedDomainEvent(Customer Customer, DateTime OccurredAtUtc) : CustomerDomainEvent(Customer, OccurredAtUtc);

public sealed record CustomerReactivatedDomainEvent(Customer Customer, DateTime OccurredAtUtc) : CustomerDomainEvent(Customer, OccurredAtUtc);

public sealed record CustomerClosedDomainEvent(Customer Customer, DateTime OccurredAtUtc) : CustomerDomainEvent(Customer, OccurredAtUtc);

public sealed record CustomerKycVerifiedDomainEvent(Customer Customer, DateTime OccurredAtUtc) : CustomerDomainEvent(Customer, OccurredAtUtc);

public sealed record CustomerKycRejectedDomainEvent(Customer Customer, DateTime OccurredAtUtc) : CustomerDomainEvent(Customer, OccurredAtUtc);
