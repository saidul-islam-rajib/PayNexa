using System.ComponentModel.DataAnnotations;

namespace PayNexa.Messaging.Kafka;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    [Required]
    public string BootstrapServers { get; set; } = string.Empty;

    public bool? AutoCreateTopics { get; set; }

    [Range(1, 100)]
    public int TopicPartitions { get; set; } = 3;

    [Range(1, 5)]
    public short TopicReplicationFactor { get; set; } = 1;

    [Range(1000, 300000)]
    public int MessageTimeoutMs { get; set; } = 10000;
}
