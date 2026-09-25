using System.Net.Mail;
using PayNexa.Customers.Domain.DomainExceptions;

namespace PayNexa.Customers.Domain.ValueObjects;

public sealed record Email
{
    public const int MaxLength = 254;

    private Email(string value) => Value = value;

    public string Value { get; }

    public static bool IsValid(string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        var trimmed = candidate.Trim();

        return trimmed.Length <= MaxLength
               && MailAddress.TryCreate(trimmed, out var address)
               && string.Equals(address.Address, trimmed, StringComparison.OrdinalIgnoreCase)
               && address.Host.Contains('.');
    }

    public static Email Create(string value) =>
        IsValid(value)
            ? new Email(value.Trim().ToLowerInvariant())
            : throw new DomainException("Customer.InvalidEmail", "The email address is not valid.");

    public override string ToString() => Value;
}
