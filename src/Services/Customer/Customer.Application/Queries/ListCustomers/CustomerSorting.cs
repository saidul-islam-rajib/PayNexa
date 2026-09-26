using PayNexa.Common.Querying;

namespace PayNexa.Customers.Application.Queries.ListCustomers;

public static class CustomerSorting
{
    public static readonly SortFieldMap<CustomerSortField> Fields = new(
        new Dictionary<string, CustomerSortField>
        {
            ["created-at"] = CustomerSortField.CreatedAt,
            ["updated-at"] = CustomerSortField.UpdatedAt,
            ["first-name"] = CustomerSortField.FirstName,
            ["last-name"] = CustomerSortField.LastName,
            ["email"] = CustomerSortField.Email,
        },
        new SortRequest<CustomerSortField>(CustomerSortField.CreatedAt, SortDirection.Descending));
}
