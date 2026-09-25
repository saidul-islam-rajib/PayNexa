using PayNexa.Customers.Domain.DomainExceptions;
using PayNexa.Customers.Domain.Entities;
using PayNexa.Customers.Domain.Enums;
using PayNexa.Customers.Domain.ValueObjects;

namespace PayNexa.Customers.UnitTests.Domain;

public sealed class CustomerTests
{
    [Fact]
    public void Create_ValidInput_CreatesActiveCustomerAtVersionOne()
    {
        var customer = CustomerTestData.Customer();

        customer.Id.ShouldNotBe(Guid.Empty);
        customer.Status.ShouldBe(CustomerStatus.Active);
        customer.Version.ShouldBe(1);
        customer.CreatedAtUtc.ShouldBe(CustomerTestData.Now);
        customer.UpdatedAtUtc.ShouldBe(CustomerTestData.Now);
    }

    [Fact]
    public void Create_TrimsNames()
    {
        var customer = Customer.Create("  Saidul ", " Rajib  ", Email.Create("a@example.com"), PhoneNumber.Create("+8801700000000"), new DateOnly(1995, 5, 20), CustomerTestData.Now);

        customer.FirstName.ShouldBe("Saidul");
        customer.LastName.ShouldBe("Rajib");
    }

    [Theory]
    [InlineData(1899, 12, 31)]
    [InlineData(2026, 9, 26)]
    public void Create_DateOfBirthOutOfRange_Throws(int year, int month, int day)
    {
        var exception = Should.Throw<DomainException>(() => Customer.Create(
            "Saidul", "Rajib", Email.Create("a@example.com"), PhoneNumber.Create("+8801700000000"), new DateOnly(year, month, day), CustomerTestData.Now));

        exception.Code.ShouldBe("Customer.InvalidDateOfBirth");
    }

    [Fact]
    public void Create_NameTooLong_Throws() =>
        Should.Throw<DomainException>(() => Customer.Create(
            new string('a', Customer.NameMaxLength + 1), "Rajib", Email.Create("a@example.com"), PhoneNumber.Create("+8801700000000"), new DateOnly(1995, 5, 20), CustomerTestData.Now))
            .Code.ShouldBe("Customer.InvalidName");

    [Fact]
    public void UpdateProfile_ChangesProfileAndIncrementsVersion()
    {
        var customer = CustomerTestData.Customer();
        var later = CustomerTestData.Now.AddMinutes(5);

        customer.UpdateProfile("Saidul", "Islam", PhoneNumber.Create("+8801711111111"), later);

        customer.FirstName.ShouldBe("Saidul");
        customer.LastName.ShouldBe("Islam");
        customer.PhoneNumber.Value.ShouldBe("+8801711111111");
        customer.UpdatedAtUtc.ShouldBe(later);
        customer.CreatedAtUtc.ShouldBe(CustomerTestData.Now);
        customer.Version.ShouldBe(2);
    }
}
