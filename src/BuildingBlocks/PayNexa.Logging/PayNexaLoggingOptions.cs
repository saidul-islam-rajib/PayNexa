using PayNexa.Logging.RequestLogging;

namespace PayNexa.Logging;

public sealed class PayNexaLoggingOptions
{
    public const string SectionName = "PayNexaLogging";

    public ConsoleLogFormat ConsoleFormat { get; set; } = ConsoleLogFormat.Text;

    public string? SeqServerUrl { get; set; }

    public string? SeqApiKey { get; set; }

    public string? FileDirectory { get; set; }

    public int RetainedFileCount { get; set; } = 7;

    public HttpBodyLoggingOptions HttpBodies { get; set; } = new();
}

public enum ConsoleLogFormat
{
    Text,
    Json,
}
