using Microsoft.AspNetCore.Http;
using PayNexa.Common.Exceptions;

namespace PayNexa.AspNetCore.ProblemDetails;

internal sealed record ExceptionMapping(int StatusCode, string ErrorCode, string Title, string Detail)
{
    public static ExceptionMapping For(Exception exception) => exception switch
    {
        DownstreamServiceException { FailureKind: DownstreamFailureKind.Timeout } downstream => new(
            StatusCodes.Status504GatewayTimeout,
            "Dependency.Timeout",
            "A dependency did not respond in time",
            $"The {downstream.TargetService} did not respond in time. Please retry later."),

        DownstreamServiceException downstream => new(
            StatusCodes.Status503ServiceUnavailable,
            "Dependency.Unavailable",
            "A required service is unavailable",
            $"The {downstream.TargetService} is temporarily unavailable. Please retry later."),

        BadHttpRequestException badRequest => new(
            badRequest.StatusCode,
            "Request.Invalid",
            "Invalid request",
            "The request could not be processed."),

        _ => new(
            StatusCodes.Status500InternalServerError,
            "Server.Unexpected",
            "Unexpected error",
            "An unexpected error occurred. Use the correlationId when contacting support."),
    };
}
