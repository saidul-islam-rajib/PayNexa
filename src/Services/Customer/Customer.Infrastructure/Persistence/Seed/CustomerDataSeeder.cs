using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayNexa.Common.Initialization;
using PayNexa.Common.Logging;
using PayNexa.Common.Persistence;
using PayNexa.Customers.Application;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Options;

namespace PayNexa.Customers.Infrastructure.Persistence.Seed;

internal sealed partial class CustomerDataSeeder(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IOptions<CustomerOptions> options,
    ILogger<CustomerDataSeeder> logger)
    : IDataSeeder
{
    public int Order => 100;

    public string Name => "Customers";

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        using var step = logger.BeginStep("Seed customers when the write store is empty");

        if (await customers.AnyAsync(cancellationToken))
        {
            step.WithProperty("Seeded", 0).Succeeded();
            return;
        }

        var seed = CustomerSeedData.Customers(options.Value.ToPolicy(), timeProvider.GetUtcNow().UtcDateTime);

        foreach (var customer in seed)
        {
            customers.Add(customer);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        step.WithProperty("Seeded", seed.Count).Succeeded();
        LogSeeded(logger, seed.Count);
    }

    [LoggerMessage(EventId = CustomerLogEvents.Seeded, Level = LogLevel.Information, Message = "Seeded {SeededCustomers} customers; read models and Kafka events follow through the outbox")]
    private static partial void LogSeeded(ILogger logger, int seededCustomers);
}
