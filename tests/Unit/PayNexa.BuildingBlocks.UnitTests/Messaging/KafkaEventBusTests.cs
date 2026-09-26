using System.Text;
using Confluent.Kafka;
using PayNexa.Common.Correlation;
using PayNexa.Messaging;
using PayNexa.Messaging.Abstractions;
using PayNexa.Messaging.Kafka;

namespace PayNexa.BuildingBlocks.UnitTests.Messaging;

public sealed class KafkaEventBusTests
{
    [IntegrationEvent("sample.created", version: 2)]
    public sealed record SampleCreated(Guid EventId, DateTime OccurredAtUtc, Guid SampleId) : IIntegrationEvent
    {
        public string PartitionKey => SampleId.ToString();
    }

    public sealed record MissingAttribute(Guid EventId, DateTime OccurredAtUtc) : IIntegrationEvent
    {
        public string PartitionKey => EventId.ToString();
    }

    private readonly IProducer<string, byte[]> _producer = Substitute.For<IProducer<string, byte[]>>();

    [Fact]
    public async Task Publish_SendsKeyedMessageWithTraceableHeadersToConventionTopic()
    {
        Message<string, byte[]>? sent = null;
        string? topic = null;
        _producer.ProduceAsync(Arg.Do<string>(value => topic = value), Arg.Do<Message<string, byte[]>>(message => sent = message), Arg.Any<CancellationToken>())
            .Returns(new DeliveryResult<string, byte[]> { Partition = new Partition(1), Offset = new Offset(42) });

        var integrationEvent = new SampleCreated(Guid.CreateVersion7(), DateTime.UtcNow, Guid.CreateVersion7());
        var bus = new KafkaEventBus(_producer, new MessagingServiceInfo("sample-service"), new FakeLogger<KafkaEventBus>());

        using (CorrelationContext.Begin("corr-001"))
        {
            await bus.PublishAsync(integrationEvent, TestContext.Current.CancellationToken);
        }

        topic.ShouldBe("paynexa.sample");
        sent.ShouldNotBeNull();
        sent.Key.ShouldBe(integrationEvent.SampleId.ToString());
        Header(sent, MessageHeaders.EventId).ShouldBe(integrationEvent.EventId.ToString());
        Header(sent, MessageHeaders.EventType).ShouldBe("sample.created");
        Header(sent, MessageHeaders.EventVersion).ShouldBe("2");
        Header(sent, MessageHeaders.CorrelationId).ShouldBe("corr-001");
        Header(sent, MessageHeaders.SourceService).ShouldBe("sample-service");
        Encoding.UTF8.GetString(sent.Value).ShouldNotContain("partitionKey");
    }

    [Fact]
    public void Metadata_WithoutAttribute_FailsFast() =>
        Should.Throw<InvalidOperationException>(() => IntegrationEventMetadata.For<MissingAttribute>());

    private static string Header(Message<string, byte[]> message, string name) =>
        Encoding.UTF8.GetString(message.Headers.GetLastBytes(name));
}
