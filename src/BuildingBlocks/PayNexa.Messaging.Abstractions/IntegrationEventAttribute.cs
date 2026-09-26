namespace PayNexa.Messaging.Abstractions;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class IntegrationEventAttribute(string eventType, int version) : Attribute
{
    public string EventType { get; } = eventType;

    public int Version { get; } = version;

    public string? Topic { get; init; }
}
