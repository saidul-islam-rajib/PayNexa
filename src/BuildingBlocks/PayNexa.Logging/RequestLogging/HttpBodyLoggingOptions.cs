namespace PayNexa.Logging.RequestLogging;

public sealed class HttpBodyLoggingOptions
{
    public bool Enabled { get; set; }

    public int MaxLoggedLength { get; set; } = 4096;

    public int MaxCapturedBytes { get; set; } = 1024 * 1024;
}
