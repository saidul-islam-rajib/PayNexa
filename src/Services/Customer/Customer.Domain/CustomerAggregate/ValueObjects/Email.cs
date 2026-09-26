using System.Net.Mail;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

public sealed class Email : SingleValueObject<string>
{
    public const int MaxLength = 254;

    private Email(string value) : base(value)
    {
    }

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
            ? new Email(Normalize(value))
            : throw new DomainException(Errors.Customer.InvalidEmail);

    public static string Normalize(string value) => value.Trim().ToLowerInvariant();
}
