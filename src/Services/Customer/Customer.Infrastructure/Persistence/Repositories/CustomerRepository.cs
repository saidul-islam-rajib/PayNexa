using Microsoft.EntityFrameworkCore;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(CustomerDbContext dbContext) : ICustomerRepository
{
    public void Add(Customer customer) => dbContext.Customers.Add(customer);

    public Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken) =>
        dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken) =>
        dbContext.Customers.AnyAsync(customer => customer.Email == email, cancellationToken);

    public Task<bool> AnyAsync(CancellationToken cancellationToken) =>
        dbContext.Customers.AnyAsync(cancellationToken);
}
