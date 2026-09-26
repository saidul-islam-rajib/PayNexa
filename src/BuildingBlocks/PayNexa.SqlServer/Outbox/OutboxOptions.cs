using System.ComponentModel.DataAnnotations;

namespace PayNexa.SqlServer.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    [Range(typeof(TimeSpan), "00:00:00.100", "00:05:00")]
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(1);

    [Range(1, 1000)]
    public int BatchSize { get; set; } = 50;

    [Range(1, 100)]
    public int MaxAttempts { get; set; } = 20;

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(5);
}
