using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayNexa.Common.HealthChecks;
using PayNexa.Common.Logging;
using PayNexa.Common.Persistence;
using PayNexa.SqlServer.Logging;
using PayNexa.SqlServer.Outbox;

namespace PayNexa.SqlServer;

public static partial class SqlServerExtensions
{
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    public static IHostApplicationBuilder AddPayNexaSqlServer<TDbContext>(
        this IHostApplicationBuilder builder,
        string connectionStringName,
        string schema)
        where TDbContext : DbContext
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringName}' is required.");
        }

        var sqlOptions = builder.Configuration.GetSection(SqlServerOptions.SectionName).Get<SqlServerOptions>() ?? new SqlServerOptions();
        var services = builder.Services;

        services.AddOptions<SqlServerOptions>().BindConfiguration(SqlServerOptions.SectionName);
        services.AddOptions<OutboxOptions>()
            .BindConfiguration(OutboxOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<SqlCommandLoggingInterceptor>();
        services.AddDbContext<TDbContext>((provider, options) => options
            .UseSqlServer(connectionString, sql => sql
                .EnableRetryOnFailure(sqlOptions.MaxRetryCount, sqlOptions.MaxRetryDelay, null)
                .MigrationsHistoryTable(MigrationsHistoryTable, schema))
            .AddInterceptors(provider.GetRequiredService<SqlCommandLoggingInterceptor>()));

        services.AddScoped<IUnitOfWork, EfUnitOfWork<TDbContext>>();
        services.AddScoped<IOutbox, EfOutbox<TDbContext>>();
        GetOrCreateRegistry(services);
        services.AddHostedService<OutboxProcessor<TDbContext>>();

        services.AddHealthChecks().AddDbContextCheck<TDbContext>(
            "sqlserver",
            tags: [HealthCheckTags.Ready],
            customTestQuery: async (dbContext, cancellationToken) =>
            {
                await dbContext.Set<OutboxMessage>().TagWith(QueryTags.BackgroundPolling).AnyAsync(cancellationToken);
                return true;
            });

        return builder;
    }

    public static IServiceCollection AddOutboxHandler<TMessage, THandler>(this IServiceCollection services)
        where TMessage : class
        where THandler : class, IOutboxMessageHandler<TMessage>
    {
        GetOrCreateRegistry(services).Register<TMessage>();
        services.AddScoped<IOutboxMessageHandler<TMessage>, THandler>();
        return services;
    }

    public static async Task ApplyDatabaseMigrationsAsync<TDbContext>(this IHost host, CancellationToken cancellationToken = default)
        where TDbContext : DbContext
    {
        var options = host.Services.GetRequiredService<IOptions<SqlServerOptions>>().Value;

        if (!options.ApplyMigrationsOnStartup)
        {
            return;
        }

        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(SqlServerExtensions).FullName!);
        const int maxAttempts = 10;

        for (var attempt = 1; ; attempt++)
        {
            await using var scope = host.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            using var step = logger.BeginStep($"Apply {typeof(TDbContext).Name} migrations");

            try
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
                step.Succeeded();
                return;
            }
            catch (Exception exception) when (attempt < maxAttempts && exception is not OperationCanceledException)
            {
                step.Failed(exception);
                LogMigrationRetry(logger, typeof(TDbContext).Name, attempt, maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    private static OutboxHandlerRegistry GetOrCreateRegistry(IServiceCollection services)
    {
        var existing = services
            .Select(descriptor => descriptor.ImplementationInstance)
            .OfType<OutboxHandlerRegistry>()
            .FirstOrDefault();

        if (existing is not null)
        {
            return existing;
        }

        var registry = new OutboxHandlerRegistry();
        services.AddSingleton(registry);
        return registry;
    }

    [LoggerMessage(EventId = LogEventIds.SqlServer + 30, Level = LogLevel.Warning,
        Message = "Database for {DbContext} not ready (attempt {Attempt}/{MaxAttempts}); retrying in 5 s")]
    private static partial void LogMigrationRetry(ILogger logger, string dbContext, int attempt, int maxAttempts);
}
