namespace PayNexa.Customers.Contracts.Requests;

public sealed record UpdateCustomerProfileRequest(
    string FirstName,
    string LastName,
    string PhoneNumber);
