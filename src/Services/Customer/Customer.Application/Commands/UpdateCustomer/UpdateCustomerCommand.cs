using Mediator;
using PayNexa.Common.Results;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.Application.Commands.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string PhoneNumber) : ICommand<Result<CustomerResponse>>;
