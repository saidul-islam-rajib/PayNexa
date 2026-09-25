namespace PayNexa.Customers.Contracts.Requests;

public sealed record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string PhoneNumber);
