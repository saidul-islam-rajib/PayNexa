using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayNexa.Common.Exceptions;
using PayNexa.Common.Logging;
using PayNexa.Common.Results;

namespace PayNexa.Common.Behaviors;

public sealed class LoggingBehavior<TMessage, TResponse>(
    ILogger<LoggingBehavior<TMessage, TResponse>> logger,
    IOptions<OperationLoggingOptions> options)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private static readonly string OperationName = typeof(TMessage).Name;
    private static readonly string OperationKind = ResolveOperationKind();

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        using var operationScope = OperationContext.Begin(OperationName);
        using var logScope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["Operation"] = OperationName,
            ["OperationKind"] = OperationKind,
        });

        OperationLog.Started(logger, OperationKind, OperationName);
        var startedAt = Stopwatch.GetTimestamp();

        try
        {
            var response = await next(message, cancellationToken);
            var durationMs = ElapsedMs(startedAt);

            if (response is Result { IsFailure: true } failed)
            {
                LogFailedResult(failed.Error, durationMs);
            }
            else
            {
                OperationLog.Succeeded(logger, OperationName, durationMs);
            }

            if (durationMs >= options.Value.SlowOperationThresholdMs)
            {
                OperationLog.Slow(logger, OperationName, durationMs, options.Value.SlowOperationThresholdMs);
            }

            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            OperationLog.Cancelled(logger, OperationName, ElapsedMs(startedAt));
            throw;
        }
        catch (Exception exception)
        {
            OperationLog.Faulted(logger, exception, OperationName, ElapsedMs(startedAt), exception.GetType().Name);
            exception.MarkAsLogged();
            throw;
        }
        finally
        {
            OperationLog.Ended(logger, OperationName);
        }
    }

    private void LogFailedResult(Error error, double durationMs)
    {
        var level = error.Type is ErrorType.Failure or ErrorType.Unavailable ? LogLevel.Error : LogLevel.Warning;
        OperationLog.FailedResult(logger, level, OperationName, durationMs, error.Code, error.Type);
    }

    private static double ElapsedMs(long startedAt) =>
        Math.Round(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, 2);

    private static string ResolveOperationKind() => typeof(TMessage) switch
    {
        var type when typeof(IBaseCommand).IsAssignableFrom(type) => "Command",
        var type when typeof(IBaseQuery).IsAssignableFrom(type) => "Query",
        _ => "Request",
    };
}

internal static partial class OperationLog
{
    [LoggerMessage(EventId = LogEventIds.Operation, Level = LogLevel.Information, Message = "{OperationKind} {Operation} started")]
    public static partial void Started(ILogger logger, string operationKind, string operation);

    [LoggerMessage(EventId = LogEventIds.Operation + 1, Level = LogLevel.Information, Message = "{Operation} succeeded in {DurationMs} ms")]
    public static partial void Succeeded(ILogger logger, string operation, double durationMs);

    [LoggerMessage(EventId = LogEventIds.Operation + 2, Message = "{Operation} failed in {DurationMs} ms with {ErrorCode} ({ErrorType})")]
    public static partial void FailedResult(ILogger logger, LogLevel level, string operation, double durationMs, string errorCode, ErrorType errorType);

    [LoggerMessage(EventId = LogEventIds.Operation + 3, Level = LogLevel.Error, Message = "{Operation} failed in {DurationMs} ms with unhandled {ExceptionType}")]
    public static partial void Faulted(ILogger logger, Exception exception, string operation, double durationMs, string exceptionType);

    [LoggerMessage(EventId = LogEventIds.Operation + 4, Level = LogLevel.Information, Message = "{Operation} cancelled after {DurationMs} ms")]
    public static partial void Cancelled(ILogger logger, string operation, double durationMs);

    [LoggerMessage(EventId = LogEventIds.Operation + 5, Level = LogLevel.Warning, Message = "{Operation} is slow: {DurationMs} ms exceeds the {SlowOperationThresholdMs} ms threshold")]
    public static partial void Slow(ILogger logger, string operation, double durationMs, int slowOperationThresholdMs);

    [LoggerMessage(EventId = LogEventIds.Operation + 6, Level = LogLevel.Information, Message = "{Operation} ended")]
    public static partial void Ended(ILogger logger, string operation);
}
