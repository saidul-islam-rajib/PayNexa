using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Exceptions;
using PayNexa.Common.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace PayNexa.Observability.ServiceClients;

public sealed partial class ServiceCallLoggingHandler(
    string sourceService,
    string targetService,
    ServiceCallMetrics metrics,
    ILogger<ServiceCallLoggingHandler> logger)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var operation = request.GetOperation();
        var method = request.Method.Method;
        var route = request.RequestUri?.AbsolutePath ?? string.Empty;
        var attempts = request.StartAttemptCounting();

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["SourceService"] = sourceService,
            ["TargetService"] = targetService,
            ["ServiceCallOperation"] = operation,
        });

        LogStarted(logger, sourceService, targetService, operation, method, route);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            LogCompleted(response, operation, method, route, ElapsedMs(startedAt), attempts.Count);
            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            LogCancelled(logger, sourceService, targetService, operation, ElapsedMs(startedAt), attempts.Count);
            throw;
        }
        catch (Exception exception) when (Classify(exception) is { } failureKind)
        {
            var durationMs = ElapsedMs(startedAt);
            metrics.RecordFailure(targetService, operation, failureKind.ToString());
            metrics.RecordDuration(targetService, operation, failureKind.ToString(), durationMs);
            LogFailedWithException(logger, exception, sourceService, targetService, operation, method, route, failureKind, durationMs, attempts.Count);

            var downstream = new DownstreamServiceException(targetService, operation, failureKind, exception);
            downstream.MarkAsLogged();
            throw downstream;
        }
        finally
        {
            LogEnded(logger, sourceService, targetService, operation);
        }
    }

    private void LogCompleted(HttpResponseMessage response, string operation, string method, string route, double durationMs, int attempts)
    {
        var statusCode = (int)response.StatusCode;

        if (statusCode >= 500)
        {
            metrics.RecordFailure(targetService, operation, $"HTTP {statusCode}");
            metrics.RecordDuration(targetService, operation, "ServerError", durationMs);
            LogFailedWithStatus(logger, sourceService, targetService, operation, method, route, statusCode, durationMs, attempts);
            return;
        }

        if (statusCode >= 400)
        {
            metrics.RecordDuration(targetService, operation, "ClientError", durationMs);
            LogClientError(logger, sourceService, targetService, operation, method, route, statusCode, durationMs, attempts);
            return;
        }

        metrics.RecordDuration(targetService, operation, "Success", durationMs);
        LogSucceeded(logger, sourceService, targetService, operation, method, route, statusCode, durationMs, attempts);
    }

    private static DownstreamFailureKind? Classify(Exception exception) => exception switch
    {
        BrokenCircuitException => DownstreamFailureKind.CircuitOpen,
        TimeoutRejectedException => DownstreamFailureKind.Timeout,
        TaskCanceledException { InnerException: TimeoutException } => DownstreamFailureKind.Timeout,
        HttpRequestException => DownstreamFailureKind.Unreachable,
        _ => null,
    };

    private static double ElapsedMs(long startedAt) => Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 2);

    [LoggerMessage(EventId = LogEventIds.ServiceCall, Level = LogLevel.Information,
        Message = "{SourceService} -> {TargetService} {ServiceCallOperation} started ({HttpMethod} {Route})")]
    private static partial void LogStarted(ILogger logger, string sourceService, string targetService, string serviceCallOperation, string httpMethod, string route);

    [LoggerMessage(EventId = LogEventIds.ServiceCall + 1, Level = LogLevel.Information,
        Message = "{SourceService} -> {TargetService} {ServiceCallOperation} succeeded: {HttpMethod} {Route} responded {StatusCode} in {DurationMs} ms after {Attempts} attempt(s)")]
    private static partial void LogSucceeded(ILogger logger, string sourceService, string targetService, string serviceCallOperation, string httpMethod, string route, int statusCode, double durationMs, int attempts);

    [LoggerMessage(EventId = LogEventIds.ServiceCall + 2, Level = LogLevel.Warning,
        Message = "{SourceService} -> {TargetService} {ServiceCallOperation} returned client error: {HttpMethod} {Route} responded {StatusCode} in {DurationMs} ms after {Attempts} attempt(s)")]
    private static partial void LogClientError(ILogger logger, string sourceService, string targetService, string serviceCallOperation, string httpMethod, string route, int statusCode, double durationMs, int attempts);

    [LoggerMessage(EventId = LogEventIds.ServiceCall + 3, Level = LogLevel.Error,
        Message = "{SourceService} -> {TargetService} {ServiceCallOperation} failed: {HttpMethod} {Route} responded {StatusCode} in {DurationMs} ms after {Attempts} attempt(s)")]
    private static partial void LogFailedWithStatus(ILogger logger, string sourceService, string targetService, string serviceCallOperation, string httpMethod, string route, int statusCode, double durationMs, int attempts);

    [LoggerMessage(EventId = LogEventIds.ServiceCall + 4, Level = LogLevel.Error,
        Message = "{SourceService} -> {TargetService} {ServiceCallOperation} failed: {HttpMethod} {Route} {FailureKind} after {DurationMs} ms and {Attempts} attempt(s). {TargetService} is unavailable")]
    private static partial void LogFailedWithException(ILogger logger, Exception exception, string sourceService, string targetService, string serviceCallOperation, string httpMethod, string route, DownstreamFailureKind failureKind, double durationMs, int attempts);

    [LoggerMessage(EventId = LogEventIds.ServiceCall + 5, Level = LogLevel.Information,
        Message = "{SourceService} -> {TargetService} {ServiceCallOperation} cancelled by caller after {DurationMs} ms and {Attempts} attempt(s)")]
    private static partial void LogCancelled(ILogger logger, string sourceService, string targetService, string serviceCallOperation, double durationMs, int attempts);

    [LoggerMessage(EventId = LogEventIds.ServiceCall + 6, Level = LogLevel.Information,
        Message = "{SourceService} -> {TargetService} {ServiceCallOperation} ended")]
    private static partial void LogEnded(ILogger logger, string sourceService, string targetService, string serviceCallOperation);
}
