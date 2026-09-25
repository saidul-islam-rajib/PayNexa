using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace PayNexa.Messaging.Abstractions;

public static class IntegrationEventSerializer
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { ExcludeTransportProperties },
        },
    };

    public static byte[] Serialize<TEvent>(TEvent integrationEvent)
        where TEvent : IIntegrationEvent =>
        JsonSerializer.SerializeToUtf8Bytes(integrationEvent, Options);

    public static TEvent? Deserialize<TEvent>(ReadOnlySpan<byte> payload)
        where TEvent : IIntegrationEvent =>
        JsonSerializer.Deserialize<TEvent>(payload, Options);

    private static void ExcludeTransportProperties(JsonTypeInfo typeInfo)
    {
        if (!typeof(IIntegrationEvent).IsAssignableFrom(typeInfo.Type))
        {
            return;
        }

        var partitionKey = typeInfo.Properties.FirstOrDefault(property =>
            string.Equals(property.Name, nameof(IIntegrationEvent.PartitionKey), StringComparison.OrdinalIgnoreCase));

        if (partitionKey is not null)
        {
            typeInfo.Properties.Remove(partitionKey);
        }
    }
}
