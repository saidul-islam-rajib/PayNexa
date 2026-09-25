using Mediator;
using PayNexa.Common.Results;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.Application.Commands.CreateCustomer;

public sealed record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth) : ICommand<Result<CustomerResponse>>;
