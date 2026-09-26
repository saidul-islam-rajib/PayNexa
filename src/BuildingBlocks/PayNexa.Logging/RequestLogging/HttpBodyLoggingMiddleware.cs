using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Logging;
using PayNexa.Logging.Masking;

namespace PayNexa.Logging.RequestLogging;

public sealed partial class HttpBodyLoggingMiddleware(
    RequestDelegate next,
    ILogger<HttpBodyLoggingMiddleware> logger,
    HttpBodyLoggingOptions options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (RequestLogLevels.IsInfrastructureEndpoint(context.Request.Path))
        {
            await next(context);
            return;
        }

        await LogRequestBodyAsync(context.Request);

        var originalBody = context.Response.Body;
        using var capturedBody = new MemoryStream();
        context.Response.Body = capturedBody;

        try
        {
            await next(context);
        }
        finally
        {
            context.Response.Body = originalBody;
            LogResponseBody(context, capturedBody);
            capturedBody.Position = 0;
            await capturedBody.CopyToAsync(originalBody, context.RequestAborted);
        }
    }

    private async Task LogRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is 0 || (request.ContentLength is null && !request.HasJsonContentType()))
        {
            return;
        }

        if (!request.HasJsonContentType())
        {
            LogRequestBody(logger, request.Method, request.Path, HttpBodyLogText.NotJson(request.ContentType, request.ContentLength ?? 0));
            return;
        }

        if (request.ContentLength > options.MaxCapturedBytes)
        {
            LogRequestBody(logger, request.Method, request.Path, HttpBodyLogText.TooLarge(request.ContentLength.Value));
            return;
        }

        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var body = await reader.ReadToEndAsync(request.HttpContext.RequestAborted);
        request.Body.Position = 0;

        LogRequestBody(logger, request.Method, request.Path, Describe(body));
    }

    private void LogResponseBody(HttpContext context, MemoryStream capturedBody)
    {
        var request = context.Request;
        var response = context.Response;

        var description = capturedBody.Length switch
        {
            0 => HttpBodyLogText.Empty,
            _ when !IsJson(response.ContentType) => HttpBodyLogText.NotJson(response.ContentType, capturedBody.Length),
            _ when capturedBody.Length > options.MaxCapturedBytes => HttpBodyLogText.TooLarge(capturedBody.Length),
            _ => Describe(Encoding.UTF8.GetString(capturedBody.GetBuffer(), 0, (int)capturedBody.Length)),
        };

        LogResponseBody(logger, request.Method, request.Path, response.StatusCode, description);
    }

    private string Describe(string body)
    {
        if (body.Length == 0)
        {
            return HttpBodyLogText.Empty;
        }

        if (!JsonBodyMasker.TryMask(body, out var masked))
        {
            return HttpBodyLogText.InvalidJson(body.Length);
        }

        return masked.Length > options.MaxLoggedLength ? HttpBodyLogText.Truncated(masked, options.MaxLoggedLength) : masked;
    }

    private static bool IsJson(string? contentType) =>
        contentType is not null
        && (contentType.StartsWith(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith(MediaTypeNames.Application.ProblemJson, StringComparison.OrdinalIgnoreCase));

    [LoggerMessage(EventId = LogEventIds.HttpRequest + 1, Level = LogLevel.Information, Message = "HTTP {RequestMethod} {RequestPath} request body: {RequestBody}")]
    private static partial void LogRequestBody(ILogger logger, string requestMethod, string requestPath, string requestBody);

    [LoggerMessage(EventId = LogEventIds.HttpRequest + 2, Level = LogLevel.Information, Message = "HTTP {RequestMethod} {RequestPath} response body ({StatusCode}): {ResponseBody}")]
    private static partial void LogResponseBody(ILogger logger, string requestMethod, string requestPath, int statusCode, string responseBody);
}
