using Mediator;
using PayNexa.Common.Querying;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.Customers.Domain.CustomerAggregate.Enums;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Queries.ListCustomers;

public sealed class ListCustomersQueryHandler(ICustomerReadStore readStore)
    : IQueryHandler<ListCustomersQuery, Result<PagedResult<CustomerResponse>>>
{
    public async ValueTask<Result<PagedResult<CustomerResponse>>> Handle(ListCustomersQuery query, CancellationToken cancellationToken)
    {
        var criteria = new CustomerListCriteria(
            query.Page,
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            QueryValidationExtensions.ParseEnumFilter<CustomerStatus>(query.Status),
            QueryValidationExtensions.ParseEnumFilter<KycStatus>(query.KycStatus),
            CustomerSorting.Fields.Resolve(query.SortBy, query.SortOrder));

        return await readStore.ListAsync(criteria, cancellationToken);
    }
}
