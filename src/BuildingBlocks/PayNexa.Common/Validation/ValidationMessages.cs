namespace PayNexa.Common.Validation;

public static class ValidationMessages
{
    public const string MaxLengthFormat = "'{{PropertyName}}' must be at most {0} characters.";
    public const string InvalidEmail = "'{PropertyName}' must be a valid email address.";
    public const string InvalidPhoneNumber = "'{PropertyName}' must be in E.164 format, e.g. +8801700000000.";
    public const string DateBetweenFormat = "'{{PropertyName}}' must be between {0} and today.";
    public const string OneOfFormat = "'{{PropertyName}}' must be one of: {0}.";
    public const string CountryCode = "'{PropertyName}' must be a two-letter ISO 3166-1 country code, e.g. BD.";
    public const string MinimumAgeFormat = "'{{PropertyName}}' must make the customer at least {0} years old.";

    public static string MaxLength(int maxLength) => string.Format(MaxLengthFormat, maxLength);

    public static string DateBetween(DateOnly earliest) => string.Format(DateBetweenFormat, earliest.ToString("yyyy-MM-dd"));

    public static string OneOf(IEnumerable<string> allowedValues) => string.Format(OneOfFormat, string.Join(", ", allowedValues));

    public static string MinimumAge(int years) => string.Format(MinimumAgeFormat, years);
}
