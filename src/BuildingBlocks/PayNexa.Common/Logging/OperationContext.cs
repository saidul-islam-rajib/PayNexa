namespace PayNexa.Common.Logging;

public static class OperationContext
{
    private static readonly AsyncLocal<string?> CurrentOperation = new();

    public static string? Current => CurrentOperation.Value;

    public static IDisposable Begin(string operation)
    {
        var previous = CurrentOperation.Value;
        CurrentOperation.Value = operation;
        return new Restore(previous);
    }

    private sealed class Restore(string? previous) : IDisposable
    {
        public void Dispose() => CurrentOperation.Value = previous;
    }
}
