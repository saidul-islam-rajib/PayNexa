namespace PayNexa.Logging.RequestLogging;

internal static class HttpBodyLogText
{
    public const string Empty = "(empty)";
    private const string NoContentType = "no content type";

    public static string NotJson(string? contentType, long length) => $"(not logged: {contentType ?? NoContentType}, {length} bytes)";

    public static string TooLarge(long length) => $"(not logged: {length} bytes exceeds the capture limit)";

    public static string InvalidJson(int length) => $"(not logged: invalid JSON, {length} chars)";

    public static string Truncated(string text, int maxLength) => $"{text[..maxLength]}... (truncated, {text.Length} chars)";
}
