using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.Policies;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.UnitTests;

internal static class CustomerTestData
{
    public static readonly DateTime Now = new(2026, 9, 25, 7, 0, 0, DateTimeKind.Utc);

    public static FakeTimeProvider TimeProvider() => new(new DateTimeOffset(Now));

    public static Address DhakaAddress() => Address.Create("Mirpur Road", null, "Dhaka", "Dhaka", "1207", "bd");

    public static Customer RegisteredCustomer(string email = "rajib@example.com", Address? address = null)
    {
        var customer = Customer.Register(
            PersonName.Create("Saidul Islam", "Rajib"),
            Email.Create(email),
            PhoneNumber.Create("+8801700000000"),
            new DateOnly(1995, 5, 20),
            address,
            CustomerPolicy.Default,
            Now);

        customer.DequeueDomainEvents();
        return customer;
    }
}
