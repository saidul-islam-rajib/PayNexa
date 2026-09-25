namespace PayNexa.Logging.Masking;

public static class PiiMasker
{
    private const int VisiblePhoneDigits = 4;

    public static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');

        if (at <= 0)
        {
            return SensitiveDataMaskingEnricher.Redacted;
        }

        return $"{email[0]}***{email[at..]}";
    }

    public static string MaskPhone(string phone)
    {
        var digits = phone.Where(char.IsAsciiDigit).ToArray();

        if (digits.Length <= VisiblePhoneDigits)
        {
            return SensitiveDataMaskingEnricher.Redacted;
        }

        return new string('*', digits.Length - VisiblePhoneDigits) + new string(digits[^VisiblePhoneDigits..]);
    }
}
