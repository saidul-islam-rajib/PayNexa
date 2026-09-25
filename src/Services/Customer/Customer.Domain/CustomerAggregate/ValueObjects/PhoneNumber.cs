using System.Text.RegularExpressions;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

public sealed partial class PhoneNumber : SingleValueObject<string>
{
    public const int MaxLength = 16;

    private PhoneNumber(string value) : base(value)
    {
    }

    public static bool IsValid(string? candidate) =>
        !string.IsNullOrWhiteSpace(candidate) && E164Pattern().IsMatch(candidate.Trim());

    public static PhoneNumber Create(string value) =>
        IsValid(value)
            ? new PhoneNumber(value.Trim())
            : throw new DomainException(Errors.Customer.InvalidPhoneNumber);

    [GeneratedRegex(@"^\+[1-9]\d{7,14}$", RegexOptions.CultureInvariant)]
    private static partial Regex E164Pattern();
}
