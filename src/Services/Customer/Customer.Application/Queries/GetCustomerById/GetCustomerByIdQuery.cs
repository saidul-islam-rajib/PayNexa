using Mediator;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Queries.GetCustomerById;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : IQuery<Result<CustomerResponse>>;
