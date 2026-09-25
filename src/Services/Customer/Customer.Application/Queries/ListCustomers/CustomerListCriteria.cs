using PayNexa.Common.Querying;

namespace PayNexa.Customers.Application.Queries.ListCustomers;

public sealed record CustomerListCriteria(
    int Page,
    int PageSize,
    string? Search,
    CustomerSortField SortBy,
    SortDirection SortDirection);
