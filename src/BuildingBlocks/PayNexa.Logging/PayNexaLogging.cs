using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PayNexa.Common.Correlation;
using PayNexa.Logging.Correlation;
using PayNexa.Logging.Masking;
using PayNexa.Logging.RequestLogging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace PayNexa.Logging;

public static class PayNexaLogging
{
    private const string TextTemplate =
        "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {ServiceName} {CorrelationId} {SourceContext}{NewLine}    {Message:lj}{NewLine}{Exception}";

    public static ILogger CreateBootstrapLogger() =>
        new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: TextTemplate)
            .CreateBootstrapLogger();

    public static IServiceCollection AddPayNexaLogging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var identity = ServiceIdentity.From(configuration, environment);
        var options = configuration.GetSection(PayNexaLoggingOptions.SectionName).Get<PayNexaLoggingOptions>()
                      ?? new PayNexaLoggingOptions();

        services.AddSingleton(identity);
        services.AddSingleton(options.HttpBodies);
        services.AddSerilog((provider, loggerConfiguration) =>
            Configure(loggerConfiguration, configuration, provider, identity, options));

        RegisterProcessLevelExceptionLogging();
        return services;
    }

    public static IApplicationBuilder UsePayNexaRequestLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestStartedLoggingMiddleware>();

        if (IsHttpBodyLoggingEnabled(app.ApplicationServices))
        {
            app.UseMiddleware<HttpBodyLoggingMiddleware>();
        }

        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.00} ms";
            options.GetLevel = (context, _, exception) => RequestLogLevels.ForCompletedRequest(context, exception);
            options.EnrichDiagnosticContext = EnrichFromRequest;
        });

        return app;
    }

    private static bool IsHttpBodyLoggingEnabled(IServiceProvider services) =>
        services.GetRequiredService<IHostEnvironment>().IsDevelopment()
        && services.GetRequiredService<HttpBodyLoggingOptions>().Enabled;

    private static void Configure(
        LoggerConfiguration configuration,
        IConfiguration appConfiguration,
        IServiceProvider services,
        ServiceIdentity identity,
        PayNexaLoggingOptions options)
    {
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Polly", LogEventLevel.Warning)
            .ReadFrom.Configuration(appConfiguration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", identity.Name)
            .Enrich.WithProperty("ServiceVersion", identity.Version)
            .Enrich.WithProperty("Environment", identity.Environment)
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.With<SensitiveDataMaskingEnricher>();

        configuration.WriteTo.Async(sink =>
        {
            if (options.ConsoleFormat == ConsoleLogFormat.Json)
            {
                sink.Console(new RenderedCompactJsonFormatter());
            }
            else
            {
                sink.Console(outputTemplate: TextTemplate);
            }
        });

        configuration.WriteTo.Async(sink => sink.File(
            new CompactJsonFormatter(),
            Path.Combine(options.FileDirectory ?? DefaultFileDirectory(identity), $"{identity.Name}-.json"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: options.RetainedFileCount,
            fileSizeLimitBytes: 100 * 1024 * 1024,
            rollOnFileSizeLimit: true));

        if (!string.IsNullOrWhiteSpace(options.SeqServerUrl))
        {
            configuration.WriteTo.Seq(options.SeqServerUrl, apiKey: options.SeqApiKey);
        }
    }

    private static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set(CorrelationContext.LogPropertyName, CorrelationContext.Current);

        var userId = httpContext.User.FindFirst("sub")?.Value;
        if (userId is not null)
        {
            diagnosticContext.Set("UserId", userId);
        }

        var endpoint = httpContext.GetEndpoint();
        if (endpoint is not null)
        {
            diagnosticContext.Set("EndpointName", endpoint.DisplayName);
        }
    }

    private static string DefaultFileDirectory(ServiceIdentity identity) =>
        Path.Combine(Path.GetTempPath(), "paynexa", "logs", identity.Name);

    private static void RegisterProcessLevelExceptionLogging()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            Log.Fatal(args.ExceptionObject as Exception, "Unhandled exception terminating the process: {IsTerminating}", args.IsTerminating);
            Log.CloseAndFlush();
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Log.Error(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };
    }
}
