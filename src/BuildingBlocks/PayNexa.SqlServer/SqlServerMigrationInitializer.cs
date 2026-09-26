using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayNexa.Common.Initialization;
using PayNexa.Common.Logging;

namespace PayNexa.SqlServer;

internal sealed partial class SqlServerMigrationInitializer<TDbContext>(
    IServiceScopeFactory scopeFactory,
    IOptions<SqlServerOptions> options,
    IHostEnvironment environment,
    ILogger<SqlServerMigrationInitializer<TDbContext>> logger)
    : IInfrastructureInitializer
    where TDbContext : DbContext
{
    private const int MaxAttempts = 12;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public int Order => InitializationOrder.WriteStore;

    public string Name => $"{typeof(TDbContext).Name} migrations";

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (!(options.Value.ApplyMigrationsOnStartup ?? environment.IsDevelopment()))
        {
            return;
        }

        for (var attempt = 1; ; attempt++)
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            using var step = logger.BeginStep($"Create or migrate {typeof(TDbContext).Name}");

            try
            {
                var pending = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
                await dbContext.Database.MigrateAsync(cancellationToken);
                step.WithProperty("AppliedMigrations", pending).Succeeded();
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts && exception is not OperationCanceledException)
            {
                step.Failed(exception);
                LogRetry(logger, typeof(TDbContext).Name, attempt, MaxAttempts, RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }
    }

    [LoggerMessage(EventId = LogEventIds.SqlServer + 30, Level = LogLevel.Warning,
        Message = "Database for {DbContext} not ready (attempt {Attempt}/{MaxAttempts}); retrying in {RetryDelaySeconds} s")]
    private static partial void LogRetry(ILogger logger, string dbContext, int attempt, int maxAttempts, double retryDelaySeconds);
}
