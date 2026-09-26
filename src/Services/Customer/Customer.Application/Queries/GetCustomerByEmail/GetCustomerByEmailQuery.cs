using Mediator;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Queries.GetCustomerByEmail;

public sealed record GetCustomerByEmailQuery(string Email) : IQuery<Result<CustomerResponse>>;
