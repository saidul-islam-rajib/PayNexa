namespace PayNexa.Common.Behaviors;

public sealed class OperationLoggingOptions
{
    public const string SectionName = "OperationLogging";

    public int SlowOperationThresholdMs { get; set; } = 500;
}
