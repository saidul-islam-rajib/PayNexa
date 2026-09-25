using System.Text.RegularExpressions;
using PayNexa.Customers.Domain.DomainExceptions;

namespace PayNexa.Customers.Domain.ValueObjects;

public sealed partial record PhoneNumber
{
    public const int MaxLength = 16;

    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    public static bool IsValid(string? candidate) =>
        !string.IsNullOrWhiteSpace(candidate) && E164Pattern().IsMatch(candidate.Trim());

    public static PhoneNumber Create(string value) =>
        IsValid(value)
            ? new PhoneNumber(value.Trim())
            : throw new DomainException("Customer.InvalidPhoneNumber", "The phone number must be in E.164 format, e.g. +8801700000000.");

    public override string ToString() => Value;

    [GeneratedRegex(@"^\+[1-9]\d{7,14}$", RegexOptions.CultureInvariant)]
    private static partial Regex E164Pattern();
}
