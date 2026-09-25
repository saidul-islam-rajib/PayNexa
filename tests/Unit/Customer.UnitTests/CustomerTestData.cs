using PayNexa.Customers.Domain.Entities;
using PayNexa.Customers.Domain.ValueObjects;

namespace PayNexa.Customers.UnitTests;

internal static class CustomerTestData
{
    public static readonly DateTime Now = new(2026, 9, 25, 7, 0, 0, DateTimeKind.Utc);

    public static FakeTimeProvider TimeProvider() => new(new DateTimeOffset(Now));

    public static Customer Customer(string email = "rajib@example.com") => PayNexa.Customers.Domain.Entities.Customer.Create(
        "Saidul Islam",
        "Rajib",
        Email.Create(email),
        PhoneNumber.Create("+8801700000000"),
        new DateOnly(1995, 5, 20),
        Now);
}
