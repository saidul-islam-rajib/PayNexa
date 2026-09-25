using PayNexa.Customers.Domain.DomainExceptions;
using PayNexa.Customers.Domain.ValueObjects;

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
    public void Email_Create_InvalidValue_ThrowsDomainException() =>
        Should.Throw<DomainException>(() => Email.Create("invalid")).Code.ShouldBe("Customer.InvalidEmail");

    [Fact]
    public void Email_EqualityIsByValue() => Email.Create("A@example.com").ShouldBe(Email.Create("a@example.com"));

    [Theory]
    [InlineData("+8801700000000")]
    [InlineData("+14155552671")]
    public void PhoneNumber_IsValid_AcceptsE164(string phone) => PhoneNumber.IsValid(phone).ShouldBeTrue();

    [Theory]
    [InlineData("01700000000")]
    [InlineData("+0123456789")]
    [InlineData("+880 1700 000000")]
    [InlineData("+1234567")]
    public void PhoneNumber_IsValid_RejectsNonE164(string phone) => PhoneNumber.IsValid(phone).ShouldBeFalse();
}
