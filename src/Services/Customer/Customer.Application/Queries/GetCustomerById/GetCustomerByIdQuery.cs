using Mediator;
using PayNexa.Common.Results;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.Application.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid Id) : IQuery<Result<CustomerResponse>>;
