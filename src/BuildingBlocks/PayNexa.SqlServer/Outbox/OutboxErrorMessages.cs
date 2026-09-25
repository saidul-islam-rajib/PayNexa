namespace PayNexa.SqlServer.Outbox;

internal static class OutboxErrorMessages
{
    public const string NullPayloadFormat = "Outbox payload for {0} deserialized to null.";
    public const string NoHandlerRegistered = "No outbox handler is registered for this message type.";
    public const string OutboxNotMappedFormat = "{0} does not map the outbox. Call modelBuilder.ApplyOutbox().";
    public const string MissingTypeNameFormat = "Outbox message type {0} has no full name.";
}
