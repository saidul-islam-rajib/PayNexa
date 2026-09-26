using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

public sealed class PersonName : ValueObject
{
    public const int MaxLength = 100;

    private PersonName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    public static bool IsValidPart(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= MaxLength;

    public static PersonName Create(string firstName, string lastName) =>
        IsValidPart(firstName) && IsValidPart(lastName)
            ? new PersonName(firstName.Trim(), lastName.Trim())
            : throw new DomainException(Errors.Customer.InvalidName);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }
}
