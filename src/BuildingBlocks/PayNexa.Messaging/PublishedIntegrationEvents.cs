using PayNexa.Messaging.Abstractions;

namespace PayNexa.Messaging;

public sealed class PublishedIntegrationEvents
{
    private readonly HashSet<Type> _eventTypes = [];

    public IReadOnlyCollection<IntegrationEventMetadata> Metadata =>
        _eventTypes.Select(IntegrationEventMetadata.For).ToArray();

    public IReadOnlyCollection<string> Topics =>
        Metadata.Select(metadata => metadata.Topic).Distinct(StringComparer.Ordinal).ToArray();

    internal void Add(Type eventType)
    {
        _ = IntegrationEventMetadata.For(eventType);
        _eventTypes.Add(eventType);
    }
}
