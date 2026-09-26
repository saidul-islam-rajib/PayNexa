using PayNexa.Customers.Contracts.Common;

namespace PayNexa.Customers.Contracts.Events;

public sealed record CustomerSnapshot(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    AddressDto? Address,
    string Status,
    string? StatusReason,
    string KycStatus,
    string? KycRejectionReason,
    DateTime CreatedAtUtc,
    string CreatedBy,
    DateTime UpdatedAtUtc,
    string UpdatedBy,
    long Version);
