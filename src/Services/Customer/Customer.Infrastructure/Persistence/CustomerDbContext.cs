using Microsoft.EntityFrameworkCore;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.SqlServer.Conventions;
using PayNexa.SqlServer.Outbox;

namespace PayNexa.Customers.Infrastructure.Persistence;

public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
{
    public const string ConnectionStringName = "CustomerDb";
    public const string Schema = "customer";

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.UseUtcDateTimes();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplySingleValueObjectConversions();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerDbContext).Assembly);
        modelBuilder.ApplyAuditingConventions();
        modelBuilder.ApplyOutbox();
    }
}
