namespace PayNexa.Messaging.Abstractions;

public static class MessageHeaders
{
    public const string EventId = "event-id";
    public const string EventType = "event-type";
    public const string EventVersion = "event-version";
    public const string OccurredAtUtc = "occurred-at-utc";
    public const string ContentType = "content-type";
    public const string CorrelationId = "correlation-id";
    public const string TraceParent = "traceparent";
    public const string SourceService = "source-service";
    public const string JsonContentType = "application/json";
}
