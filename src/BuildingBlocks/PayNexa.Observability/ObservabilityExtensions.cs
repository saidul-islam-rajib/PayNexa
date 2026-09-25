using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PayNexa.Logging;
using PayNexa.Observability.ServiceClients;

namespace PayNexa.Observability;

public static class ObservabilityExtensions
{
    private static readonly PathString[] UntracedPaths = ["/health", "/openapi", "/swagger"];

    public static IServiceCollection AddPayNexaObservability(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var identity = ServiceIdentity.From(configuration, environment);
        var options = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>()
                      ?? new ObservabilityOptions();

        services.AddSingleton<ServiceCallMetrics>();
        services.AddPayNexaHealthChecks();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(identity.Name, serviceVersion: identity.Version)
                .AddAttributes([new KeyValuePair<string, object>("deployment.environment.name", identity.Environment)]))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(PayNexaTelemetry.ActivitySourceWildcard)
                    .AddAspNetCoreInstrumentation(instrumentation =>
                        instrumentation.Filter = context => !UntracedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
                    .AddHttpClientInstrumentation();

                if (options.OtlpTracesEndpoint is not null)
                {
                    tracing.AddOtlpExporter(exporter => ConfigureExporter(exporter, options.OtlpTracesEndpoint, options.OtlpHeaders));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(PayNexaTelemetry.MeterWildcard)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                if (options.OtlpMetricsEndpoint is not null)
                {
                    metrics.AddOtlpExporter(exporter => ConfigureExporter(exporter, options.OtlpMetricsEndpoint, options.OtlpHeaders));
                }
            });

        return services;
    }

    private static void ConfigureExporter(OtlpExporterOptions exporter, Uri endpoint, string? headers)
    {
        exporter.Endpoint = endpoint;
        exporter.Protocol = OtlpExportProtocol.HttpProtobuf;

        if (!string.IsNullOrWhiteSpace(headers))
        {
            exporter.Headers = headers;
        }
    }
}
