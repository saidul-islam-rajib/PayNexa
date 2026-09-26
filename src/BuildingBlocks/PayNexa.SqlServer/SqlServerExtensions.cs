using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PayNexa.Common.HealthChecks;
using PayNexa.Common.Initialization;
using PayNexa.Common.Persistence;
using PayNexa.Common.Security;
using PayNexa.SqlServer.Auditing;
using PayNexa.SqlServer.Logging;
using PayNexa.SqlServer.Outbox;

namespace PayNexa.SqlServer;

public static class SqlServerExtensions
{
    public const string MigrationsHistoryTable = "__EFMigrationsHistory";

    private static string WithPassword(string connectionString, string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return connectionString;
        }

        return new SqlConnectionStringBuilder(connectionString) { Password = password }.ConnectionString;
    }

    public static IServiceCollection AddPayNexaSqlServer<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName,
        string schema)
        where TDbContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(string.Format(SqlServerErrorMessages.ConnectionStringRequiredFormat, connectionStringName));
        }

        var sqlSection = configuration.GetSection(SqlServerOptions.SectionName);
        var sqlOptions = sqlSection.Get<SqlServerOptions>() ?? new SqlServerOptions();
        connectionString = WithPassword(connectionString, sqlOptions.Password);

        services.AddOptions<SqlServerOptions>().Bind(sqlSection);
        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<ICurrentActor, SystemActor>();
        services.AddScoped<AuditStamper>();
        services.AddSingleton<SqlCommandLoggingInterceptor>();
        services.AddDbContext<TDbContext>((provider, options) => options
            .UseSqlServer(connectionString, sql => sql
                .EnableRetryOnFailure(sqlOptions.MaxRetryCount, sqlOptions.MaxRetryDelay, null)
                .MigrationsHistoryTable(MigrationsHistoryTable, schema))
            .AddInterceptors(provider.GetRequiredService<SqlCommandLoggingInterceptor>()));

        services.AddScoped<IUnitOfWork, EfUnitOfWork<TDbContext>>();
        services.AddScoped<IOutbox, EfOutbox<TDbContext>>();
        services.AddSingleton(_ => OutboxHandlerRegistry.FromServices(services));
        services.AddHostedService<OutboxProcessor<TDbContext>>();
        services.AddSingleton<IInfrastructureInitializer, SqlServerMigrationInitializer<TDbContext>>();

        services.AddHealthChecks().AddDbContextCheck<TDbContext>(
            "sqlserver",
            tags: [HealthCheckTags.Ready],
            customTestQuery: async (dbContext, cancellationToken) =>
            {
                await dbContext.Set<OutboxMessage>().TagWith(QueryTags.BackgroundPolling).AnyAsync(cancellationToken);
                return true;
            });

        return services;
    }
}
