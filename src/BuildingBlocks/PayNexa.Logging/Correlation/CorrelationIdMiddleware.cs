using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Correlation;
using PayNexa.Common.Logging;
using Serilog.Context;

namespace PayNexa.Logging.Correlation;

public sealed partial class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationContext.HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        Activity.Current?.SetTag(CorrelationContext.ActivityTagName, correlationId);

        using (CorrelationContext.Begin(correlationId))
        using (LogContext.PushProperty(CorrelationContext.LogPropertyName, correlationId))
        using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
        {
            await next(context);
        }
    }

    private string ResolveCorrelationId(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationContext.HeaderName, out var values) || values.Count == 0)
        {
            return CorrelationContext.NewId();
        }

        var candidate = values[0];

        if (CorrelationContext.IsValid(candidate))
        {
            return candidate!;
        }

        var generated = CorrelationContext.NewId();
        LogRejectedCorrelationId(logger, CorrelationContext.HeaderName, generated);
        return generated;
    }

    [LoggerMessage(EventId = LogEventIds.HttpRequest + 10, Level = LogLevel.Warning,
        Message = "Rejected malformed {HeaderName} header; generated {CorrelationId} instead")]
    private static partial void LogRejectedCorrelationId(ILogger logger, string headerName, string correlationId);
}
