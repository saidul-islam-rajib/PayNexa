using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Logging;

namespace PayNexa.Logging.RequestLogging;

public sealed partial class RequestStartedLoggingMiddleware(RequestDelegate next, ILogger<RequestStartedLoggingMiddleware> logger)
{
    public Task InvokeAsync(HttpContext context)
    {
        var level = RequestLogLevels.IsInfrastructureEndpoint(context.Request.Path) ? LogLevel.Debug : LogLevel.Information;
        LogRequestStarted(logger, level, context.Request.Method, context.Request.Path);
        return next(context);
    }

    [LoggerMessage(EventId = LogEventIds.HttpRequest, Message = "HTTP {RequestMethod} {RequestPath} started")]
    private static partial void LogRequestStarted(ILogger logger, LogLevel level, string requestMethod, string requestPath);
}
