namespace PayNexa.Common.Correlation;

public static class CorrelationContext
{
    public const string HeaderName = "X-Correlation-Id";
    public const string LogPropertyName = "CorrelationId";
    public const string ActivityTagName = "correlation.id";

    private static readonly AsyncLocal<string?> CurrentCorrelationId = new();

    public static string? Current => CurrentCorrelationId.Value;

    public static IDisposable Begin(string correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        var previous = CurrentCorrelationId.Value;
        CurrentCorrelationId.Value = correlationId;
        return new Restore(previous);
    }

    public static string NewId() => Guid.CreateVersion7().ToString("N");

    public static bool IsValid(string? candidate) =>
        !string.IsNullOrWhiteSpace(candidate)
        && candidate.Length <= 64
        && candidate.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' or ':');

    private sealed class Restore(string? previous) : IDisposable
    {
        public void Dispose() => CurrentCorrelationId.Value = previous;
    }
}
