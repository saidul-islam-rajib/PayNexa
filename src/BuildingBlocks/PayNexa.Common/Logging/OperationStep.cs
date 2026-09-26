using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PayNexa.Common.Logging;

public sealed class OperationStep : IDisposable
{
    private const string AbandonedReason = "Step ended without an explicit outcome (exception or early return)";

    private readonly ILogger _logger;
    private readonly string _operation;
    private readonly string _step;
    private readonly long _startedAt;
    private readonly IDisposable? _scope;
    private Dictionary<string, object?>? _properties;
    private bool _completed;
    private bool _disposed;

    internal OperationStep(ILogger logger, string step)
    {
        _logger = logger;
        _step = step;
        _operation = OperationContext.Current ?? "-";
        _scope = logger.BeginScope(new Dictionary<string, object?> { ["Step"] = step });
        StepLog.Started(logger, _operation, step);
        _startedAt = Stopwatch.GetTimestamp();
    }

    public OperationStep WithProperty(string name, object? value)
    {
        (_properties ??= new Dictionary<string, object?>(StringComparer.Ordinal))[name] = value;
        return this;
    }

    public void Succeeded()
    {
        if (!TryComplete())
        {
            return;
        }

        using (BeginOutcomeScope())
        {
            StepLog.Succeeded(_logger, _operation, _step, ElapsedMs());
        }
    }

    public void Failed(string reason)
    {
        if (!TryComplete())
        {
            return;
        }

        using (BeginOutcomeScope())
        {
            StepLog.Failed(_logger, _operation, _step, ElapsedMs(), reason);
        }
    }

    public void Failed(Exception exception, LogLevel level = LogLevel.Warning)
    {
        if (!TryComplete())
        {
            return;
        }

        using (BeginOutcomeScope())
        {
            StepLog.FailedWithException(_logger, level, exception, _operation, _step, ElapsedMs(), exception.GetType().Name);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (!_completed)
        {
            Failed(AbandonedReason);
        }

        StepLog.Ended(_logger, _operation, _step);
        _scope?.Dispose();
    }

    private bool TryComplete()
    {
        if (_completed)
        {
            return false;
        }

        _completed = true;
        return true;
    }

    private double ElapsedMs() => Math.Round(Stopwatch.GetElapsedTime(_startedAt).TotalMilliseconds, 2);

    private IDisposable? BeginOutcomeScope() => _properties is null ? null : _logger.BeginScope(_properties);
}

public static class OperationStepExtensions
{
    public static OperationStep BeginStep(this ILogger logger, string step)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(step);
        return new OperationStep(logger, step);
    }
}

internal static partial class StepLog
{
    [LoggerMessage(EventId = LogEventIds.Step, Level = LogLevel.Information, Message = "{Operation}: {Step} started")]
    public static partial void Started(ILogger logger, string operation, string step);

    [LoggerMessage(EventId = LogEventIds.Step + 1, Level = LogLevel.Information, Message = "{Operation}: {Step} succeeded in {DurationMs} ms")]
    public static partial void Succeeded(ILogger logger, string operation, string step, double durationMs);

    [LoggerMessage(EventId = LogEventIds.Step + 2, Level = LogLevel.Warning, Message = "{Operation}: {Step} failed in {DurationMs} ms. Reason: {FailureReason}")]
    public static partial void Failed(ILogger logger, string operation, string step, double durationMs, string failureReason);

    [LoggerMessage(EventId = LogEventIds.Step + 3, Message = "{Operation}: {Step} failed in {DurationMs} ms with {ExceptionType}")]
    public static partial void FailedWithException(ILogger logger, LogLevel level, Exception exception, string operation, string step, double durationMs, string exceptionType);

    [LoggerMessage(EventId = LogEventIds.Step + 4, Level = LogLevel.Information, Message = "{Operation}: {Step} ended")]
    public static partial void Ended(ILogger logger, string operation, string step);
}
