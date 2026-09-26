using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate.Enums;
using PayNexa.Customers.Domain.CustomerAggregate.Events;
using PayNexa.Customers.Domain.CustomerAggregate.Policies;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Domain;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Domain.CustomerAggregate;

public sealed class Customer : AggregateRoot<CustomerId>
{
    public static readonly DateOnly EarliestDateOfBirth = new(1900, 1, 1);

    private Customer(CustomerId id, PersonName name, Email email, PhoneNumber phoneNumber, DateOnly dateOfBirth)
        : base(id)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        DateOfBirth = dateOfBirth;
        Status = CustomerStatus.Active;
        KycStatus = KycStatus.Pending;
    }

    private Customer()
    {
        Name = null!;
        Email = null!;
        PhoneNumber = null!;
    }

    public PersonName Name { get; private set; }

    public Email Email { get; private set; }

    public PhoneNumber PhoneNumber { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public Address? Address { get; private set; }

    public CustomerStatus Status { get; private set; }

    public StatusReason? StatusReason { get; private set; }

    public KycStatus KycStatus { get; private set; }

    public StatusReason? KycRejectionReason { get; private set; }

    public static Customer Register(
        PersonName name,
        Email email,
        PhoneNumber phoneNumber,
        DateOnly dateOfBirth,
        Address? address,
        CustomerPolicy policy,
        DateTime utcNow,
        CustomerId? id = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phoneNumber);
        ArgumentNullException.ThrowIfNull(policy);

        EnsureValidDateOfBirth(dateOfBirth, policy, utcNow);

        var customer = new Customer(id ?? CustomerId.CreateUnique(), name, email, phoneNumber, dateOfBirth);
        customer.Address = address;
        customer.Raise(new CustomerRegisteredDomainEvent(customer, utcNow));
        return customer;
    }

    public void UpdateProfile(PersonName name, PhoneNumber phoneNumber, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(phoneNumber);
        EnsureNotClosed();

        Name = name;
        PhoneNumber = phoneNumber;
        Raise(new CustomerProfileUpdatedDomainEvent(this, utcNow));
    }

    public void ChangeEmail(Email email, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(email);
        EnsureNotClosed();

        Email = email;
        Raise(new CustomerEmailChangedDomainEvent(this, utcNow));
    }

    public void ChangeAddress(Address address, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(address);
        EnsureNotClosed();

        Address = address;
        Raise(new CustomerAddressChangedDomainEvent(this, utcNow));
    }

    public void Suspend(StatusReason reason, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(reason);
        EnsureNotClosed();

        if (Status != CustomerStatus.Active)
        {
            throw new DomainException(Errors.Customer.NotActive);
        }

        Status = CustomerStatus.Suspended;
        StatusReason = reason;
        Raise(new CustomerSuspendedDomainEvent(this, utcNow));
    }

    public void Reactivate(DateTime utcNow)
    {
        EnsureNotClosed();

        if (Status != CustomerStatus.Suspended)
        {
            throw new DomainException(Errors.Customer.NotSuspended);
        }

        Status = CustomerStatus.Active;
        StatusReason = null;
        Raise(new CustomerReactivatedDomainEvent(this, utcNow));
    }

    public void Close(StatusReason reason, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(reason);
        EnsureNotClosed();

        Status = CustomerStatus.Closed;
        StatusReason = reason;
        Raise(new CustomerClosedDomainEvent(this, utcNow));
    }

    public void VerifyKyc(DateTime utcNow)
    {
        EnsureNotClosed();

        if (KycStatus == KycStatus.Verified)
        {
            throw new DomainException(Errors.Customer.KycAlreadyVerified);
        }

        if (Address is null)
        {
            throw new DomainException(Errors.Customer.AddressRequiredForKyc);
        }

        KycStatus = KycStatus.Verified;
        KycRejectionReason = null;
        Raise(new CustomerKycVerifiedDomainEvent(this, utcNow));
    }

    public void RejectKyc(StatusReason reason, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(reason);
        EnsureNotClosed();

        if (KycStatus != KycStatus.Pending)
        {
            throw new DomainException(Errors.Customer.KycNotPending);
        }

        KycStatus = KycStatus.Rejected;
        KycRejectionReason = reason;
        Raise(new CustomerKycRejectedDomainEvent(this, utcNow));
    }

    public PaymentEligibility EvaluatePaymentEligibility()
    {
        var reasons = new List<Error>();

        if (Status != CustomerStatus.Active)
        {
            reasons.Add(Status == CustomerStatus.Closed ? Errors.Customer.Closed : Errors.Customer.NotActive);
        }

        if (KycStatus != KycStatus.Verified)
        {
            reasons.Add(Errors.Customer.KycNotVerified);
        }

        return PaymentEligibility.From(reasons);
    }

    public static bool IsValidDateOfBirth(DateOnly dateOfBirth, DateTime utcNow) =>
        dateOfBirth >= EarliestDateOfBirth && dateOfBirth <= DateOnly.FromDateTime(utcNow);

    private static void EnsureValidDateOfBirth(DateOnly dateOfBirth, CustomerPolicy policy, DateTime utcNow)
    {
        if (!IsValidDateOfBirth(dateOfBirth, utcNow))
        {
            throw new DomainException(Errors.Customer.InvalidDateOfBirth);
        }

        if (!policy.MeetsMinimumAge(dateOfBirth, DateOnly.FromDateTime(utcNow)))
        {
            throw new DomainException(Errors.Customer.BelowMinimumAge(policy.MinimumAgeYears));
        }
    }

    private void EnsureNotClosed()
    {
        if (Status == CustomerStatus.Closed)
        {
            throw new DomainException(Errors.Customer.Closed);
        }
    }
}
