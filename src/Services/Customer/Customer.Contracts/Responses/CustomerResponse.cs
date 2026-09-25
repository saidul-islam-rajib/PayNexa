namespace PayNexa.Customers.Contracts.Responses;

public sealed record CustomerResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
