namespace PayNexa.SqlServer;

public sealed class SqlServerOptions
{
    public const string SectionName = "SqlServer";

    public bool? ApplyMigrationsOnStartup { get; set; }

    public string? Password { get; set; }

    public int MaxRetryCount { get; set; } = 5;

    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(10);
}
