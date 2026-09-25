using System.Diagnostics;
using System.Globalization;
using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Correlation;
using PayNexa.Common.Logging;
using PayNexa.Messaging.Abstractions;

namespace PayNexa.Messaging.Kafka;

internal sealed class KafkaEventBus(
    IProducer<string, byte[]> producer,
    MessagingServiceInfo serviceInfo,
    ILogger<KafkaEventBus> logger)
    : IEventBus
{
    public const string ActivitySourceName = "PayNexa.Messaging";

    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken)
        where TEvent : class, IIntegrationEvent
    {
        var metadata = IntegrationEventMetadata.For<TEvent>();

        using var activity = ActivitySource.StartActivity($"{metadata.Topic} publish", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "kafka");
        activity?.SetTag("messaging.destination.name", metadata.Topic);
        activity?.SetTag("messaging.message.id", integrationEvent.EventId.ToString());

        using var step = logger.BeginStep($"Kafka publish {metadata.EventType} to {metadata.Topic}")
            .WithProperty("EventId", integrationEvent.EventId)
            .WithProperty("EventType", metadata.EventType)
            .WithProperty("EventVersion", metadata.Version)
            .WithProperty("Topic", metadata.Topic)
            .WithProperty("MessageKey", integrationEvent.PartitionKey);

        var message = new Message<string, byte[]>
        {
            Key = integrationEvent.PartitionKey,
            Value = IntegrationEventSerializer.Serialize(integrationEvent),
            Timestamp = new Timestamp(integrationEvent.OccurredAtUtc),
            Headers = BuildHeaders(integrationEvent, metadata),
        };

        var delivery = await producer.ProduceAsync(metadata.Topic, message, cancellationToken);

        step.WithProperty("Partition", delivery.Partition.Value)
            .WithProperty("Offset", delivery.Offset.Value)
            .Succeeded();
    }

    private Headers BuildHeaders<TEvent>(TEvent integrationEvent, IntegrationEventMetadata metadata)
        where TEvent : IIntegrationEvent
    {
        var headers = new Headers
        {
            { MessageHeaders.EventId, Utf8(integrationEvent.EventId.ToString()) },
            { MessageHeaders.EventType, Utf8(metadata.EventType) },
            { MessageHeaders.EventVersion, Utf8(metadata.Version.ToString(CultureInfo.InvariantCulture)) },
            { MessageHeaders.OccurredAtUtc, Utf8(integrationEvent.OccurredAtUtc.ToString("O", CultureInfo.InvariantCulture)) },
            { MessageHeaders.ContentType, Utf8(MessageHeaders.JsonContentType) },
            { MessageHeaders.SourceService, Utf8(serviceInfo.ServiceName) },
        };

        if (CorrelationContext.Current is { } correlationId)
        {
            headers.Add(MessageHeaders.CorrelationId, Utf8(correlationId));
        }

        if (Activity.Current?.Id is { } traceParent)
        {
            headers.Add(MessageHeaders.TraceParent, Utf8(traceParent));
        }

        return headers;
    }

    private static byte[] Utf8(string value) => Encoding.UTF8.GetBytes(value);
}
