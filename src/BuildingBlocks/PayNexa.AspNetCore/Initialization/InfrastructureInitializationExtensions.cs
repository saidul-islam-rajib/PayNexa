using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Correlation;
using PayNexa.Common.Initialization;
using PayNexa.Common.Logging;

namespace PayNexa.AspNetCore.Initialization;

public static class InfrastructureInitializationExtensions
{
    private const string LoggerCategory = "PayNexa.Initialization";

    public static async Task InitializeInfrastructureAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(LoggerCategory);
        using var operation = OperationContext.Begin("Startup");
        using var correlation = CorrelationContext.Begin(CorrelationContext.NewId());
        using var scope = logger.BeginScope(new Dictionary<string, object?> { [CorrelationContext.LogPropertyName] = CorrelationContext.Current });

        foreach (var initializer in host.Services.GetServices<IInfrastructureInitializer>().OrderBy(initializer => initializer.Order))
        {
            using var step = logger.BeginStep($"Initialize {initializer.Name}");
            await initializer.InitializeAsync(cancellationToken);
            step.Succeeded();
        }

        if (!host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            return;
        }

        await using var seedScope = host.Services.CreateAsyncScope();

        foreach (var seeder in seedScope.ServiceProvider.GetServices<IDataSeeder>().OrderBy(seeder => seeder.Order))
        {
            using var step = logger.BeginStep($"Seed {seeder.Name}");
            await seeder.SeedAsync(cancellationToken);
            step.Succeeded();
        }
    }
}
