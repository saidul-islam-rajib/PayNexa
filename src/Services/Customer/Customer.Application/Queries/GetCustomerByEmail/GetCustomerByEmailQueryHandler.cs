using Mediator;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Queries.GetCustomerByEmail;

public sealed class GetCustomerByEmailQueryHandler(ICustomerReadStore readStore)
    : IQueryHandler<GetCustomerByEmailQuery, Result<CustomerResponse>>
{
    public async ValueTask<Result<CustomerResponse>> Handle(GetCustomerByEmailQuery query, CancellationToken cancellationToken)
    {
        var customer = await readStore.GetByEmailAsync(Email.Normalize(query.Email), cancellationToken);
        return customer is null ? Errors.Customer.NotFoundByEmail : customer;
    }
}
