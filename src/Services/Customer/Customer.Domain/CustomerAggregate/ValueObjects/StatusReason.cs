using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

public sealed class StatusReason : SingleValueObject<string>
{
    public const int MaxLength = 500;

    private StatusReason(string value) : base(value)
    {
    }

    public static bool IsValid(string? candidate) =>
        !string.IsNullOrWhiteSpace(candidate) && candidate.Trim().Length <= MaxLength;

    public static StatusReason Create(string value) =>
        IsValid(value)
            ? new StatusReason(value.Trim())
            : throw new DomainException(Errors.Customer.InvalidReason);
}
