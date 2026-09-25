using PayNexa.Customers.Domain.Entities;
using PayNexa.Customers.Domain.ValueObjects;

namespace PayNexa.Customers.Application.Interfaces;

public interface ICustomerRepository
{
    void Add(Customer customer);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken);
}
