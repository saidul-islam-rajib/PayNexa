using Mediator;
using PayNexa.Customers.Contracts.Common;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Commands.RegisterCustomer;

public sealed record RegisterCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    AddressDto? Address) : ICommand<Result<CustomerResponse>>;
