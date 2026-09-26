namespace PayNexa.SqlServer.Outbox;

public sealed class OutboxMessage
{
    public const int TypeMaxLength = 300;
    public const int CorrelationIdMaxLength = 64;
    public const int TraceParentMaxLength = 128;
    public const int LastErrorMaxLength = 2000;

    public Guid Id { get; init; }

    public required string Type { get; init; }

    public required string Payload { get; init; }

    public DateTime OccurredAtUtc { get; init; }

    public string? CorrelationId { get; init; }

    public string? TraceParent { get; init; }

    public DateTime? ProcessedAtUtc { get; set; }

    public int Attempts { get; set; }

    public DateTime? NextAttemptAtUtc { get; set; }

    public string? LastError { get; set; }
}
