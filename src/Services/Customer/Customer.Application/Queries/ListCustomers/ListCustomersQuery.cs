using Mediator;
using PayNexa.Common.Querying;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Queries.ListCustomers;

public sealed record ListCustomersQuery(
    PageRequest Page,
    string? Search,
    string? Status,
    string? KycStatus,
    string? SortBy,
    string? SortOrder) : IQuery<Result<PagedResult<CustomerResponse>>>;
