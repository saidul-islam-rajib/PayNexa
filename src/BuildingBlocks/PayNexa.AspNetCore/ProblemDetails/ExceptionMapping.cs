using Microsoft.AspNetCore.Http;
using PayNexa.Common.Exceptions;
using PayNexa.SharedKernel.Results;

namespace PayNexa.AspNetCore.ProblemDetails;

internal sealed record ExceptionMapping(int StatusCode, string ErrorCode, string Title, string Detail)
{
    public static ExceptionMapping For(Exception exception) => exception switch
    {
        DownstreamServiceException { FailureKind: DownstreamFailureKind.Timeout } downstream => new(
            StatusCodes.Status504GatewayTimeout,
            CommonErrorCodes.DependencyTimeout,
            CommonErrorMessages.TitleDependencyTimeout,
            string.Format(CommonErrorMessages.DependencyTimeoutFormat, downstream.TargetService)),

        DownstreamServiceException downstream => new(
            StatusCodes.Status503ServiceUnavailable,
            CommonErrorCodes.DependencyUnavailable,
            CommonErrorMessages.TitleDependencyUnavailable,
            string.Format(CommonErrorMessages.DependencyUnavailableFormat, downstream.TargetService)),

        BadHttpRequestException badRequest => new(
            badRequest.StatusCode,
            CommonErrorCodes.RequestInvalid,
            CommonErrorMessages.TitleRequestInvalid,
            CommonErrorMessages.RequestInvalid),

        _ => new(
            StatusCodes.Status500InternalServerError,
            CommonErrorCodes.ServerUnexpected,
            CommonErrorMessages.TitleUnexpected,
            CommonErrorMessages.ServerUnexpected),
    };
}
