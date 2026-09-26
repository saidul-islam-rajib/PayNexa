using PayNexa.Common.Querying;
using PayNexa.Customers.Application.Queries.ListCustomers;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.Application.Interfaces;

public interface ICustomerReadStore
{
    Task<CustomerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<CustomerResponse?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<PagedResult<CustomerResponse>> ListAsync(CustomerListCriteria criteria, CancellationToken cancellationToken);
}
