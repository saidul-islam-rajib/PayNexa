using Mediator;
using PayNexa.Common.Results;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.Application.Queries.ListCustomers;

public sealed record ListCustomersQuery(
    int Page,
    int PageSize,
    string? Search,
    string? SortBy,
    string? SortOrder) : IQuery<Result<PagedResult<CustomerResponse>>>
{
    public const int MaxPageSize = 100;
    public const int MaxSearchLength = 100;
}
