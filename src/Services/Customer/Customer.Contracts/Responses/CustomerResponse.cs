using PayNexa.Customers.Contracts.Common;

namespace PayNexa.Customers.Contracts.Responses;

public sealed record CustomerResponse(
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
    string UpdatedBy);
