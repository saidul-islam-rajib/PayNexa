namespace PayNexa.Messaging.Abstractions;

public static class MessagingErrorMessages
{
    public const string MissingIntegrationEventAttributeFormat = "Integration event {0} must be decorated with [IntegrationEvent(eventType, version)].";
    public const string MissingServiceName = "Configuration value 'Service:Name' is required to identify the publishing service.";
}
