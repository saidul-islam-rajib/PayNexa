using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.Customers.UnitTests.Domain;

public sealed class ValueObjectTests
{
    [Theory]
    [InlineData("rajib@example.com")]
    [InlineData("first.last+tag@sub.example.co")]
    public void Email_IsValid_AcceptsWellFormedAddresses(string email) => Email.IsValid(email).ShouldBeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("rajib@localhost")]
    [InlineData("Rajib <rajib@example.com>")]
    public void Email_IsValid_RejectsMalformedAddresses(string? email) => Email.IsValid(email).ShouldBeFalse();

    [Fact]
    public void Email_Create_NormalizesToTrimmedLowerCase() =>
        Email.Create("  Saidul.Is.Rajib@Gmail.com ").Value.ShouldBe("saidul.is.rajib@gmail.com");

    [Fact]
    public void Email_Create_InvalidValue_ThrowsDomainExceptionWithError() =>
        Should.Throw<DomainException>(() => Email.Create("invalid")).Error.Code.ShouldBe(CustomerErrorCodes.InvalidEmail);

    [Fact]
    public void SingleValueObjects_CompareByValue()
    {
        Email.Create("A@example.com").ShouldBe(Email.Create("a@example.com"));
        PhoneNumber.Create("+8801700000000").ShouldBe(PhoneNumber.Create(" +8801700000000 "));
        Email.Create("a@example.com").ToString().ShouldBe("a@example.com");
    }

    [Theory]
    [InlineData("01700000000")]
    [InlineData("+0123456789")]
    [InlineData("+880 1700 000000")]
    public void PhoneNumber_IsValid_RejectsNonE164(string phone) => PhoneNumber.IsValid(phone).ShouldBeFalse();

    [Fact]
    public void PersonName_Create_TrimsAndComposesFullName()
    {
        var name = PersonName.Create("  Saidul Islam ", " Rajib ");

        name.FullName.ShouldBe("Saidul Islam Rajib");
        name.ShouldBe(PersonName.Create("Saidul Islam", "Rajib"));
    }

    [Fact]
    public void Address_Create_NormalizesCountryCodeAndOptionalParts()
    {
        var address = Address.Create(" Mirpur Road ", "  ", "Dhaka", null, "1207", "bd");

        address.CountryCode.ShouldBe("BD");
        address.Line1.ShouldBe("Mirpur Road");
        address.Line2.ShouldBeNull();
        address.ShouldBe(Address.Create("Mirpur Road", null, "Dhaka", null, "1207", "BD"));
    }

    [Theory]
    [InlineData("", "Dhaka", "1207", "BD")]
    [InlineData("Road", "", "1207", "BD")]
    [InlineData("Road", "Dhaka", "", "BD")]
    [InlineData("Road", "Dhaka", "1207", "BGD")]
    [InlineData("Road", "Dhaka", "1207", "B1")]
    public void Address_InvalidParts_ThrowDomainException(string line1, string city, string postalCode, string country) =>
        Should.Throw<DomainException>(() => Address.Create(line1, null, city, null, postalCode, country))
            .Error.Code.ShouldBe(CustomerErrorCodes.InvalidAddress);

    [Fact]
    public void StatusReason_TooLong_ThrowsDomainException() =>
        Should.Throw<DomainException>(() => StatusReason.Create(new string('x', StatusReason.MaxLength + 1)))
            .Error.Code.ShouldBe(CustomerErrorCodes.InvalidReason);

    [Fact]
    public void CustomerId_EqualityIsByValue()
    {
        var value = Guid.CreateVersion7();

        CustomerId.Create(value).ShouldBe(CustomerId.Create(value));
        CustomerId.CreateUnique().ShouldNotBe(CustomerId.CreateUnique());
    }
}
