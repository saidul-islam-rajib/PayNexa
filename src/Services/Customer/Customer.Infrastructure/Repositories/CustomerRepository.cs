using Microsoft.EntityFrameworkCore;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Domain.Entities;
using PayNexa.Customers.Domain.ValueObjects;
using PayNexa.Customers.Infrastructure.Persistence;

namespace PayNexa.Customers.Infrastructure.Repositories;

internal sealed class CustomerRepository(CustomerDbContext dbContext) : ICustomerRepository
{
    public void Add(Customer customer) => dbContext.Customers.Add(customer);

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken) =>
        dbContext.Customers.AnyAsync(customer => customer.Email == email, cancellationToken);
}
