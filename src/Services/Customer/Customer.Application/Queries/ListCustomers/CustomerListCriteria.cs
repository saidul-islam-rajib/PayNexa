using PayNexa.Common.Querying;
using PayNexa.Customers.Domain.CustomerAggregate.Enums;

namespace PayNexa.Customers.Application.Queries.ListCustomers;

public sealed record CustomerListCriteria(
    PageRequest Page,
    string? Search,
    CustomerStatus? Status,
    KycStatus? KycStatus,
    SortRequest<CustomerSortField> Sort);
