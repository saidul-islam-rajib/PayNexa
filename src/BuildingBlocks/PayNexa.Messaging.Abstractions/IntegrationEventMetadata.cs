using System.Collections.Concurrent;
using System.Reflection;

namespace PayNexa.Messaging.Abstractions;

public sealed record IntegrationEventMetadata(string EventType, int Version, string Topic)
{
    public const string TopicPrefix = "paynexa.";

    private static readonly ConcurrentDictionary<Type, IntegrationEventMetadata> Cache = new();

    public static IntegrationEventMetadata For<TEvent>()
        where TEvent : IIntegrationEvent =>
        For(typeof(TEvent));

    public static IntegrationEventMetadata For(Type eventType) => Cache.GetOrAdd(eventType, Resolve);

    private static IntegrationEventMetadata Resolve(Type eventType)
    {
        var attribute = eventType.GetCustomAttribute<IntegrationEventAttribute>()
                        ?? throw new InvalidOperationException(string.Format(MessagingErrorMessages.MissingIntegrationEventAttributeFormat, eventType.Name));

        return new IntegrationEventMetadata(attribute.EventType, attribute.Version, attribute.Topic ?? TopicFor(attribute.EventType));
    }

    private static string TopicFor(string eventType)
    {
        var separator = eventType.LastIndexOf('.');
        return TopicPrefix + (separator > 0 ? eventType[..separator] : eventType);
    }
}
