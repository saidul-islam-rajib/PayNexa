namespace PayNexa.SharedKernel.Results;

public static class CommonErrorMessages
{
    public const string ValidationFailed = "One or more validation errors occurred.";
    public const string RequestInvalid = "The request could not be processed.";
    public const string DependencyTimeoutFormat = "The {0} did not respond in time. Please retry later.";
    public const string DependencyUnavailableFormat = "The {0} is temporarily unavailable. Please retry later.";
    public const string ServerUnexpected = "An unexpected error occurred. Use the correlationId when contacting support.";

    public const string TitleValidation = "Validation failed";
    public const string TitleUnauthorized = "Authentication required";
    public const string TitleForbidden = "Access denied";
    public const string TitleNotFound = "Resource not found";
    public const string TitleConflict = "Request conflicts with the current state";
    public const string TitleBusinessRule = "Business rule violated";
    public const string TitleUnavailable = "Service temporarily unavailable";
    public const string TitleUnexpected = "Unexpected error";
    public const string TitleDependencyTimeout = "A dependency did not respond in time";
    public const string TitleDependencyUnavailable = "A required service is unavailable";
    public const string TitleRequestInvalid = "Invalid request";
}
