using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PayNexa.Observability;

/// <summary>
/// Standard health endpoints every PayNexa service exposes (requirements §24).
/// </summary>
public static class HealthCheckExtensions
{
    public const string LiveTag = "live";
    public const string ReadyTag = "ready";

    /// <summary>
    /// Registers the liveness check. Services chain their dependency checks
    /// (SQL Server, MongoDB, Redis, Kafka, Vault) onto the returned builder tagged with <see cref="ReadyTag"/>,
    /// so a failing dependency makes the service not-ready without marking the process dead.
    /// </summary>
    public static IHealthChecksBuilder AddPayNexaHealthChecks(this IServiceCollection services) =>
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: [LiveTag]);

    public static IEndpointRouteBuilder MapPayNexaHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health");
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = check => check.Tags.Contains(LiveTag) });
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains(ReadyTag) });

        return endpoints;
    }
}
