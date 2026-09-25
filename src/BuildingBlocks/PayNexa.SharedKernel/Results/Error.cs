namespace PayNexa.SharedKernel.Results;

public sealed record Error
{
    private Error(string code, string description, ErrorType type, IReadOnlyDictionary<string, string[]>? validationErrors)
    {
        Code = code;
        Description = description;
        Type = type;
        ValidationErrors = validationErrors;
    }

    public string Code { get; }

    public string Description { get; }

    public ErrorType Type { get; }

    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure, null);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound, null);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict, null);

    public static Error BusinessRule(string code, string description) => new(code, description, ErrorType.BusinessRule, null);

    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized, null);

    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden, null);

    public static Error Unavailable(string code, string description) => new(code, description, ErrorType.Unavailable, null);

    public static Error Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(CommonErrorCodes.ValidationFailed, CommonErrorMessages.ValidationFailed, ErrorType.Validation, errors);
}
