using Microsoft.Extensions.Logging;
using PayNexa.Common.Logging;

namespace PayNexa.Messaging.Kafka;

internal static partial class KafkaLog
{
    [LoggerMessage(EventId = LogEventIds.Kafka, Level = LogLevel.Warning,
        Message = "Kafka client error {KafkaErrorCode}: {KafkaErrorReason} (fatal: {IsFatal})")]
    public static partial void ClientError(ILogger logger, string kafkaErrorCode, string kafkaErrorReason, bool isFatal);
}
