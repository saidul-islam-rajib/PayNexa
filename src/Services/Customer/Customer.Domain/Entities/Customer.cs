using PayNexa.Customers.Domain.DomainExceptions;
using PayNexa.Customers.Domain.Enums;
using PayNexa.Customers.Domain.ValueObjects;

namespace PayNexa.Customers.Domain.Entities;

public sealed class Customer
{
    public const int NameMaxLength = 100;

    public static readonly DateOnly EarliestDateOfBirth = new(1900, 1, 1);

    private Customer()
    {
    }

    public Guid Id { get; private set; }

    public string FirstName { get; private set; } = null!;

    public string LastName { get; private set; } = null!;

    public Email Email { get; private set; } = null!;

    public PhoneNumber PhoneNumber { get; private set; } = null!;

    public DateOnly DateOfBirth { get; private set; }

    public CustomerStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public long Version { get; private set; }

    public static Customer Create(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber phoneNumber,
        DateOnly dateOfBirth,
        DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phoneNumber);
        EnsureValidDateOfBirth(dateOfBirth, utcNow);

        return new Customer
        {
            Id = Guid.CreateVersion7(),
            FirstName = NormalizeName(firstName, "first name"),
            LastName = NormalizeName(lastName, "last name"),
            Email = email,
            PhoneNumber = phoneNumber,
            DateOfBirth = dateOfBirth,
            Status = CustomerStatus.Active,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            Version = 1,
        };
    }

    public void UpdateProfile(string firstName, string lastName, PhoneNumber phoneNumber, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);

        if (Status == CustomerStatus.Closed)
        {
            throw new DomainException("Customer.Closed", "A closed customer cannot be updated.");
        }

        FirstName = NormalizeName(firstName, "first name");
        LastName = NormalizeName(lastName, "last name");
        PhoneNumber = phoneNumber;
        UpdatedAtUtc = utcNow;
        Version++;
    }

    public static bool IsValidDateOfBirth(DateOnly dateOfBirth, DateTime utcNow) =>
        dateOfBirth >= EarliestDateOfBirth && dateOfBirth <= DateOnly.FromDateTime(utcNow);

    private static void EnsureValidDateOfBirth(DateOnly dateOfBirth, DateTime utcNow)
    {
        if (!IsValidDateOfBirth(dateOfBirth, utcNow))
        {
            throw new DomainException("Customer.InvalidDateOfBirth", "The date of birth must be between 1900-01-01 and today.");
        }
    }

    private static string NormalizeName(string value, string fieldName)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > NameMaxLength)
        {
            throw new DomainException("Customer.InvalidName", $"The {fieldName} is required and must be at most {NameMaxLength} characters.");
        }

        return trimmed;
    }
}
