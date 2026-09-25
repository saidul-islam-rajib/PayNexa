using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Logging;
using PayNexa.Logging;
using Polly;

namespace PayNexa.Observability.ServiceClients;

public static partial class ServiceClientExtensions
{
    private const string ResilienceLoggerCategory = "PayNexa.ServiceCalls.Resilience";

    public static IHttpClientBuilder AddServiceClient<TClient, TImplementation>(
        this IServiceCollection services,
        string targetService,
        Uri baseAddress,
        Action<HttpStandardResilienceOptions>? configureResilience = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetService);
        ArgumentNullException.ThrowIfNull(baseAddress);

        services.AddTransient<OutboundAttemptHandler>();

        var builder = services
            .AddHttpClient<TClient, TImplementation>(client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler(provider => new ServiceCallLoggingHandler(
                provider.GetRequiredService<ServiceIdentity>().Name,
                targetService,
                provider.GetRequiredService<ServiceCallMetrics>(),
                provider.GetRequiredService<ILogger<ServiceCallLoggingHandler>>()));

        builder.AddStandardResilienceHandler()
            .Configure((options, provider) =>
            {
                options.Retry.DisableForUnsafeHttpMethods();
                configureResilience?.Invoke(options);
                AttachResilienceLogging(options, targetService, provider);
            });

        builder.AddHttpMessageHandler<OutboundAttemptHandler>();

        return builder;
    }

    private static void AttachResilienceLogging(HttpStandardResilienceOptions options, string targetService, IServiceProvider provider)
    {
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(ResilienceLoggerCategory);
        var metrics = provider.GetRequiredService<ServiceCallMetrics>();
        var sourceService = provider.GetRequiredService<ServiceIdentity>().Name;

        options.Retry.OnRetry = args =>
        {
            metrics.RecordRetry(targetService);
            LogRetrying(logger, sourceService, targetService, args.AttemptNumber + 1, Describe(args.Outcome), Math.Round(args.RetryDelay.TotalMilliseconds));
            return ValueTask.CompletedTask;
        };

        options.CircuitBreaker.OnOpened = args =>
        {
            metrics.RecordCircuitOpened(targetService);
            LogCircuitOpened(logger, targetService, Describe(args.Outcome), args.BreakDuration.TotalSeconds);
            return ValueTask.CompletedTask;
        };

        options.CircuitBreaker.OnHalfOpened = _ =>
        {
            LogCircuitHalfOpened(logger, targetService);
            return ValueTask.CompletedTask;
        };

        options.CircuitBreaker.OnClosed = _ =>
        {
            LogCircuitClosed(logger, targetService);
            return ValueTask.CompletedTask;
        };
    }

    private static string Describe(Outcome<HttpResponseMessage> outcome) =>
        outcome.Exception is not null
            ? outcome.Exception.GetType().Name
            : $"HTTP {(int)outcome.Result!.StatusCode}";

    [LoggerMessage(EventId = LogEventIds.ServiceCall + 10, Level = LogLevel.Warning,
        Message = "{SourceService} -> {TargetService} attempt {Attempt} failed with {FailureReason}; retrying in {RetryDelayMs} ms")]
    private static partial void LogRetrying(ILogger logger, string sourceService, string targetService, int attempt, string failureReason, double retryDelayMs);

    [LoggerMessage(EventId = LogEventIds.ServiceCall + 11, Level = LogLevel.Error,
        Message = "Circuit OPENED for {TargetService} after {FailureReason}; calls are rejected for {BreakDurationSeconds} s. {TargetService} is considered down")]
    private static partial void LogCircuitOpened(ILogger logger, string targetService, string failureReason, double breakDurationSeconds);

    [LoggerMessage(EventId = LogEventIds.ServiceCall + 12, Level = LogLevel.Warning,
        Message = "Circuit HALF-OPEN for {TargetService}; probing whether it recovered")]
    private static partial void LogCircuitHalfOpened(ILogger logger, string targetService);

    [LoggerMessage(EventId = LogEventIds.ServiceCall + 13, Level = LogLevel.Information,
        Message = "Circuit CLOSED for {TargetService}; calls flow normally again")]
    private static partial void LogCircuitClosed(ILogger logger, string targetService);
}
