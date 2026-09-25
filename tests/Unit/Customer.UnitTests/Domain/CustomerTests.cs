using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.Enums;
using PayNexa.Customers.Domain.CustomerAggregate.Events;
using PayNexa.Customers.Domain.CustomerAggregate.Policies;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.Customers.UnitTests.Domain;

public sealed class CustomerTests
{
    private static readonly DateTime Later = CustomerTestData.Now.AddMinutes(5);

    [Fact]
    public void Register_CreatesActivePendingKycCustomerAndRaisesRegisteredEvent()
    {
        var customer = Register(new DateOnly(1995, 5, 20), CustomerTestData.DhakaAddress());

        customer.Status.ShouldBe(CustomerStatus.Active);
        customer.KycStatus.ShouldBe(KycStatus.Pending);
        customer.Address.ShouldBe(CustomerTestData.DhakaAddress());
        customer.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<CustomerRegisteredDomainEvent>().Customer.ShouldBeSameAs(customer);
    }

    [Fact]
    public void Register_BelowMinimumAge_ThrowsDomainException() =>
        Should.Throw<DomainException>(() => Register(new DateOnly(2010, 1, 1)))
            .Error.Code.ShouldBe(CustomerErrorCodes.BelowMinimumAge);

    [Fact]
    public void Register_ExactlyMinimumAge_IsAllowed() =>
        Register(new DateOnly(2008, 9, 25)).Status.ShouldBe(CustomerStatus.Active);

    [Theory]
    [InlineData(1899, 12, 31)]
    [InlineData(2026, 9, 26)]
    public void Register_DateOfBirthOutOfRange_ThrowsDomainException(int year, int month, int day) =>
        Should.Throw<DomainException>(() => Register(new DateOnly(year, month, day)))
            .Error.Code.ShouldBe(CustomerErrorCodes.InvalidDateOfBirth);

    [Fact]
    public void UpdateProfile_ChangesNameAndPhoneAndRaisesEvent()
    {
        var customer = CustomerTestData.RegisteredCustomer();

        customer.UpdateProfile(PersonName.Create("Saidul", "Islam"), PhoneNumber.Create("+8801711111111"), Later);

        customer.Name.ShouldBe(PersonName.Create("Saidul", "Islam"));
        customer.PhoneNumber.Value.ShouldBe("+8801711111111");
        customer.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<CustomerProfileUpdatedDomainEvent>();
    }

    [Fact]
    public void ChangeEmail_And_ChangeAddress_RaiseTheirOwnEvents()
    {
        var customer = CustomerTestData.RegisteredCustomer();

        customer.ChangeEmail(Email.Create("new@example.com"), Later);
        customer.ChangeAddress(CustomerTestData.DhakaAddress(), Later);

        customer.Email.Value.ShouldBe("new@example.com");
        customer.DomainEvents.Select(domainEvent => domainEvent.GetType())
            .ShouldBe([typeof(CustomerEmailChangedDomainEvent), typeof(CustomerAddressChangedDomainEvent)]);
    }

    [Fact]
    public void Suspend_ThenReactivate_ReturnsToActive()
    {
        var customer = CustomerTestData.RegisteredCustomer();

        customer.Suspend(StatusReason.Create("Suspicious activity"), Later);
        customer.Status.ShouldBe(CustomerStatus.Suspended);
        customer.StatusReason!.Value.ShouldBe("Suspicious activity");

        customer.Reactivate(Later);
        customer.Status.ShouldBe(CustomerStatus.Active);
        customer.StatusReason.ShouldBeNull();
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ThrowsNotActive()
    {
        var customer = CustomerTestData.RegisteredCustomer();
        customer.Suspend(StatusReason.Create("First"), Later);

        Should.Throw<DomainException>(() => customer.Suspend(StatusReason.Create("Second"), Later))
            .Error.Code.ShouldBe(CustomerErrorCodes.NotActive);
    }

    [Fact]
    public void Reactivate_WhenActive_ThrowsNotSuspended() =>
        Should.Throw<DomainException>(() => CustomerTestData.RegisteredCustomer().Reactivate(Later))
            .Error.Code.ShouldBe(CustomerErrorCodes.NotSuspended);

    [Fact]
    public void Close_IsTerminal()
    {
        var customer = CustomerTestData.RegisteredCustomer();
        customer.Close(StatusReason.Create("Customer request"), Later);

        customer.Status.ShouldBe(CustomerStatus.Closed);
        Should.Throw<DomainException>(() => customer.UpdateProfile(PersonName.Create("A", "B"), PhoneNumber.Create("+8801700000000"), Later))
            .Error.Code.ShouldBe(CustomerErrorCodes.Closed);
        Should.Throw<DomainException>(() => customer.Reactivate(Later)).Error.Code.ShouldBe(CustomerErrorCodes.Closed);
    }

    [Fact]
    public void VerifyKyc_WithoutAddress_ThrowsAddressRequired() =>
        Should.Throw<DomainException>(() => CustomerTestData.RegisteredCustomer().VerifyKyc(Later))
            .Error.Code.ShouldBe(CustomerErrorCodes.AddressRequiredForKyc);

    [Fact]
    public void VerifyKyc_WithAddress_VerifiesOnce()
    {
        var customer = CustomerTestData.RegisteredCustomer(address: CustomerTestData.DhakaAddress());

        customer.VerifyKyc(Later);

        customer.KycStatus.ShouldBe(KycStatus.Verified);
        Should.Throw<DomainException>(() => customer.VerifyKyc(Later)).Error.Code.ShouldBe(CustomerErrorCodes.KycAlreadyVerified);
    }

    [Fact]
    public void RejectKyc_OnlyWhenPending_AndCanBeVerifiedLater()
    {
        var customer = CustomerTestData.RegisteredCustomer(address: CustomerTestData.DhakaAddress());

        customer.RejectKyc(StatusReason.Create("Document unreadable"), Later);
        customer.KycStatus.ShouldBe(KycStatus.Rejected);
        Should.Throw<DomainException>(() => customer.RejectKyc(StatusReason.Create("Again"), Later)).Error.Code.ShouldBe(CustomerErrorCodes.KycNotPending);

        customer.VerifyKyc(Later);
        customer.KycStatus.ShouldBe(KycStatus.Verified);
        customer.KycRejectionReason.ShouldBeNull();
    }

    [Fact]
    public void PaymentEligibility_RequiresActiveStatusAndVerifiedKyc()
    {
        var customer = CustomerTestData.RegisteredCustomer(address: CustomerTestData.DhakaAddress());

        customer.EvaluatePaymentEligibility().Reasons.Select(reason => reason.Code).ShouldBe([CustomerErrorCodes.KycNotVerified]);

        customer.VerifyKyc(Later);
        customer.EvaluatePaymentEligibility().IsEligible.ShouldBeTrue();

        customer.Suspend(StatusReason.Create("Review"), Later);
        var eligibility = customer.EvaluatePaymentEligibility();
        eligibility.IsEligible.ShouldBeFalse();
        eligibility.Reasons.Select(reason => reason.Code).ShouldBe([CustomerErrorCodes.NotActive]);
    }

    private static Customer Register(DateOnly dateOfBirth, Address? address = null) => Customer.Register(
        PersonName.Create("A", "B"),
        Email.Create("a@example.com"),
        PhoneNumber.Create("+8801700000000"),
        dateOfBirth,
        address,
        CustomerPolicy.Default,
        CustomerTestData.Now);
}
