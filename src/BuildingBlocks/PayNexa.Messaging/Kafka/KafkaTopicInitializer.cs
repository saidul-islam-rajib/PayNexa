using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayNexa.Common.Initialization;
using PayNexa.Common.Logging;

namespace PayNexa.Messaging.Kafka;

internal sealed class KafkaTopicInitializer(
    IAdminClient adminClient,
    PublishedIntegrationEvents publishedEvents,
    IOptions<KafkaOptions> options,
    IHostEnvironment environment,
    ILogger<KafkaTopicInitializer> logger)
    : IInfrastructureInitializer
{
    public int Order => InitializationOrder.MessageBroker;

    public string Name => "Kafka topics";

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;

        if (!(settings.AutoCreateTopics ?? environment.IsDevelopment()) || publishedEvents.Topics.Count == 0)
        {
            return;
        }

        var existing = adminClient.GetMetadata(TimeSpan.FromSeconds(10)).Topics
            .Select(topic => topic.Topic)
            .ToHashSet(StringComparer.Ordinal);

        var missing = publishedEvents.Topics
            .Where(topic => !existing.Contains(topic))
            .Select(topic => new TopicSpecification
            {
                Name = topic,
                NumPartitions = settings.TopicPartitions,
                ReplicationFactor = settings.TopicReplicationFactor,
            })
            .ToList();

        using var step = logger.BeginStep("Create Kafka topics")
            .WithProperty("Topics", missing.Select(topic => topic.Name).ToArray());

        if (missing.Count > 0)
        {
            try
            {
                await adminClient.CreateTopicsAsync(missing);
            }
            catch (CreateTopicsException exception) when (exception.Results.All(result => result.Error.Code is ErrorCode.NoError or ErrorCode.TopicAlreadyExists))
            {
            }
        }

        step.Succeeded();
    }
}
