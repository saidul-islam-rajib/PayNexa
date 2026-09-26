using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.Policies;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.Infrastructure.Persistence.Seed;

internal static class CustomerSeedData
{
    public static readonly Guid RajibId = new("58c49479-ec65-4de2-86e7-033c546291aa");
    public static readonly Guid TestId = new("189dc8dc-990f-48e0-a37b-e6f2b60b9d7d");

    public static IReadOnlyList<Customer> Customers(CustomerPolicy policy, DateTime utcNow)
    {
        var rajib = Customer.Register(
            PersonName.Create("Saidul Islam", "Rajib"),
            Email.Create("saidul.is.rajib@gmail.com"),
            PhoneNumber.Create("+8801700000000"),
            new DateOnly(1995, 5, 20),
            Address.Create("Mirpur Road", null, "Dhaka", "Dhaka", "1207", "BD"),
            policy,
            utcNow,
            CustomerId.Create(RajibId));
        rajib.VerifyKyc(utcNow);

        var test = Customer.Register(
            PersonName.Create("Test", "Customer"),
            Email.Create("test@gmail.com"),
            PhoneNumber.Create("+8801800000000"),
            new DateOnly(1990, 1, 1),
            Address.Create("Shyamoli Road", null, "Dhaka", "Dhaka", "1207", "BD"),
            policy,
            utcNow,
            CustomerId.Create(TestId));

        return [rajib, test];
    }
}
