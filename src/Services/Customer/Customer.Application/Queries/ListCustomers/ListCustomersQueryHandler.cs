using Mediator;
using PayNexa.Common.Querying;
using PayNexa.Common.Results;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.Application.Queries.ListCustomers;

public sealed class ListCustomersQueryHandler(ICustomerReadStore readStore)
    : IQueryHandler<ListCustomersQuery, Result<PagedResult<CustomerResponse>>>
{
    public async ValueTask<Result<PagedResult<CustomerResponse>>> Handle(ListCustomersQuery query, CancellationToken cancellationToken)
    {
        CustomerSortOptions.TryParseField(query.SortBy, out var sortBy);
        CustomerSortOptions.TryParseDirection(query.SortOrder, out var sortDirection);

        var criteria = new CustomerListCriteria(
            query.Page,
            query.PageSize,
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            sortBy ?? CustomerSortField.CreatedAt,
            sortDirection ?? SortDirection.Descending);

        return await readStore.ListAsync(criteria, cancellationToken);
    }
}
