using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PayNexa.SqlServer;

namespace PayNexa.Customers.Infrastructure.Persistence;

internal sealed class CustomerDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CustomerDbContext>
{
    public CustomerDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CustomerDbContext>()
            .UseSqlServer(
                "Server=design-time-only;Database=CustomerDb",
                sql => sql.MigrationsHistoryTable(SqlServerExtensions.MigrationsHistoryTable, CustomerDbContext.Schema))
            .Options;

        return new CustomerDbContext(options);
    }
}
