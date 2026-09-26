using Microsoft.AspNetCore.Http;
using PayNexa.SharedKernel.Results;

namespace PayNexa.AspNetCore.ProblemDetails;

public static class ErrorHttpMapping
{
    public static int StatusCodeFor(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
        ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError,
    };

    public static string TitleFor(ErrorType type) => type switch
    {
        ErrorType.Validation => CommonErrorMessages.TitleValidation,
        ErrorType.Unauthorized => CommonErrorMessages.TitleUnauthorized,
        ErrorType.Forbidden => CommonErrorMessages.TitleForbidden,
        ErrorType.NotFound => CommonErrorMessages.TitleNotFound,
        ErrorType.Conflict => CommonErrorMessages.TitleConflict,
        ErrorType.BusinessRule => CommonErrorMessages.TitleBusinessRule,
        ErrorType.Unavailable => CommonErrorMessages.TitleUnavailable,
        _ => CommonErrorMessages.TitleUnexpected,
    };
}
