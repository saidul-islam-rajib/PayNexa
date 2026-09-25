using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Exceptions;
using PayNexa.Common.Logging;

namespace PayNexa.AspNetCore.ProblemDetails;

public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    private const int ClientClosedRequest = 499;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            LogClientAborted(logger, httpContext.Request.Method, httpContext.Request.Path);
            httpContext.Response.StatusCode = ClientClosedRequest;
            return true;
        }

        var mapping = ExceptionMapping.For(exception);

        if (exception.IsLogged())
        {
            LogAlreadyLoggedException(logger, exception.GetType().Name, mapping.StatusCode, mapping.ErrorCode);
        }
        else
        {
            LogUnhandledException(logger, exception, exception.GetType().Name, mapping.StatusCode, mapping.ErrorCode);
        }

        httpContext.Response.StatusCode = mapping.StatusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Status = mapping.StatusCode,
                Title = mapping.Title,
                Detail = mapping.Detail,
                Extensions = { [ProblemDetailsExtensions.ErrorCodeExtension] = mapping.ErrorCode },
            },
        });
    }

    [LoggerMessage(EventId = LogEventIds.Exception, Level = LogLevel.Error,
        Message = "Unhandled {ExceptionType} mapped to HTTP {StatusCode} ({ErrorCode})")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string exceptionType, int statusCode, string errorCode);

    [LoggerMessage(EventId = LogEventIds.Exception + 1, Level = LogLevel.Error,
        Message = "Previously logged {ExceptionType} mapped to HTTP {StatusCode} ({ErrorCode})")]
    private static partial void LogAlreadyLoggedException(ILogger logger, string exceptionType, int statusCode, string errorCode);

    [LoggerMessage(EventId = LogEventIds.Exception + 2, Level = LogLevel.Information,
        Message = "HTTP {RequestMethod} {RequestPath} aborted by the client")]
    private static partial void LogClientAborted(ILogger logger, string requestMethod, string requestPath);
}
