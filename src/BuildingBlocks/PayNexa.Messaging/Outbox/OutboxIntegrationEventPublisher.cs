using PayNexa.Common.Persistence;
using PayNexa.Messaging.Abstractions;

namespace PayNexa.Messaging.Outbox;

internal sealed class OutboxIntegrationEventPublisher<TEvent>(IEventBus eventBus) : IOutboxMessageHandler<TEvent>
    where TEvent : class, IIntegrationEvent
{
    public Task HandleAsync(TEvent message, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(message, cancellationToken);
}
