namespace PayNexa.Logging.Masking;

public static class SensitiveFields
{
    private static readonly string[] SecretNameFragments =
    [
        "password", "secret", "token", "apikey", "api_key", "authorization", "credential",
        "cardnumber", "card_number", "cvv", "cvc", "primaryaccountnumber", "connectionstring", "privatekey",
    ];

    private static readonly string[] PersonalNameFragments =
    [
        "firstname", "lastname", "fullname", "dateofbirth", "line1", "line2", "postalcode",
    ];

    public static SensitiveFieldKind Classify(string name)
    {
        if (IsSecret(name))
        {
            return SensitiveFieldKind.Secret;
        }

        if (name.Contains("email", StringComparison.OrdinalIgnoreCase))
        {
            return SensitiveFieldKind.Email;
        }

        if (name.Contains("phone", StringComparison.OrdinalIgnoreCase))
        {
            return SensitiveFieldKind.Phone;
        }

        return PersonalNameFragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase))
            ? SensitiveFieldKind.Personal
            : SensitiveFieldKind.None;
    }

    public static string Mask(SensitiveFieldKind kind, string value) => kind switch
    {
        SensitiveFieldKind.Secret => SensitiveDataMaskingEnricher.Redacted,
        SensitiveFieldKind.Email => PiiMasker.MaskEmail(value),
        SensitiveFieldKind.Phone => PiiMasker.MaskPhone(value),
        SensitiveFieldKind.Personal => PiiMasker.MaskPersonal(value),
        _ => value,
    };

    private static bool IsSecret(string name) =>
        SecretNameFragments.Any(fragment => name.Contains(fragment, StringComparison.OrdinalIgnoreCase))
        && !name.EndsWith("ExpiresAtUtc", StringComparison.OrdinalIgnoreCase);
}
