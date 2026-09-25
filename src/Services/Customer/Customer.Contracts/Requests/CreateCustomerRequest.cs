namespace PayNexa.Customers.Contracts.Requests;

public sealed record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth);
