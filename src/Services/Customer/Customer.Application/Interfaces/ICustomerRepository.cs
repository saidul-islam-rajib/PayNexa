using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.Application.Interfaces;

public interface ICustomerRepository
{
    void Add(Customer customer);

    Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken);

    Task<bool> AnyAsync(CancellationToken cancellationToken);
}
