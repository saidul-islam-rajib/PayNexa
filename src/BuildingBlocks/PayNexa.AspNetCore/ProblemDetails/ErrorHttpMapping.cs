using Microsoft.AspNetCore.Http;
using PayNexa.Common.Results;

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
        ErrorType.Validation => "Validation failed",
        ErrorType.Unauthorized => "Authentication required",
        ErrorType.Forbidden => "Access denied",
        ErrorType.NotFound => "Resource not found",
        ErrorType.Conflict => "Request conflicts with the current state",
        ErrorType.BusinessRule => "Business rule violated",
        ErrorType.Unavailable => "Service temporarily unavailable",
        _ => "Unexpected error",
    };
}
