using PayNexa.Customers.Contracts.Common;

namespace PayNexa.Customers.Contracts.Requests;

public sealed record RegisterCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    AddressDto? Address);
