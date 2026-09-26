using Microsoft.AspNetCore.Http;
using Serilog.Events;

namespace PayNexa.Logging.RequestLogging;

internal static class RequestLogLevels
{
    private static readonly PathString[] InfrastructurePaths = ["/health", "/openapi", "/swagger"];

    public static bool IsInfrastructureEndpoint(PathString path) =>
        InfrastructurePaths.Any(prefix => path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));

    public static LogEventLevel ForCompletedRequest(HttpContext context, Exception? exception)
    {
        if (exception is not null || context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            return LogEventLevel.Error;
        }

        if (IsInfrastructureEndpoint(context.Request.Path))
        {
            return LogEventLevel.Debug;
        }

        return context.Response.StatusCode >= StatusCodes.Status400BadRequest
            ? LogEventLevel.Warning
            : LogEventLevel.Information;
    }
}
