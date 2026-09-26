using PayNexa.SharedKernel.Domain;

namespace PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

public sealed class CustomerId : StronglyTypedId
{
    private CustomerId(Guid value) : base(value)
    {
    }

    public static CustomerId CreateUnique() => new(Guid.CreateVersion7());

    public static CustomerId Create(Guid value) => new(value);
}
