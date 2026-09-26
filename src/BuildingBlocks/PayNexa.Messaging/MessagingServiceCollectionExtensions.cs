using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayNexa.Common.Configuration;
using PayNexa.Common.HealthChecks;
using PayNexa.Common.Initialization;
using PayNexa.Common.Persistence;
using PayNexa.Messaging.Abstractions;
using PayNexa.Messaging.Kafka;
using PayNexa.Messaging.Outbox;

namespace PayNexa.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddPayNexaKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration[ServiceConfigurationKeys.ServiceName] ?? throw new InvalidOperationException(MessagingErrorMessages.MissingServiceName);

        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(new MessagingServiceInfo(serviceName));
        GetOrCreatePublishedEvents(services);

        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<KafkaOptions>>().Value;
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("PayNexa.Messaging.Kafka.Producer");

            return new ProducerBuilder<string, byte[]>(new ProducerConfig
                {
                    BootstrapServers = options.BootstrapServers,
                    ClientId = serviceName,
                    Acks = Acks.All,
                    EnableIdempotence = true,
                    MessageTimeoutMs = options.MessageTimeoutMs,
                    LingerMs = 5,
                    CompressionType = CompressionType.Lz4,
                })
                .SetErrorHandler((_, error) => KafkaLog.ClientError(logger, error.Code.ToString(), error.Reason, error.IsFatal))
                .Build();
        });

        services.AddSingleton(provider => new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = provider.GetRequiredService<IOptions<KafkaOptions>>().Value.BootstrapServers,
                ClientId = serviceName,
            })
            .Build());

        services.AddSingleton<IEventBus, KafkaEventBus>();
        services.AddSingleton<IInfrastructureInitializer, KafkaTopicInitializer>();
        services.AddHealthChecks().AddCheck<KafkaHealthCheck>("kafka", tags: [HealthCheckTags.Ready]);

        return services;
    }

    public static IServiceCollection AddIntegrationEventPublishing<TEvent>(this IServiceCollection services)
        where TEvent : class, IIntegrationEvent
    {
        GetOrCreatePublishedEvents(services).Add(typeof(TEvent));
        return services.AddOutboxHandler<TEvent, OutboxIntegrationEventPublisher<TEvent>>();
    }

    private static PublishedIntegrationEvents GetOrCreatePublishedEvents(IServiceCollection services)
    {
        var existing = services.Select(descriptor => descriptor.ImplementationInstance).OfType<PublishedIntegrationEvents>().FirstOrDefault();

        if (existing is not null)
        {
            return existing;
        }

        var created = new PublishedIntegrationEvents();
        services.AddSingleton(created);
        return created;
    }
}
