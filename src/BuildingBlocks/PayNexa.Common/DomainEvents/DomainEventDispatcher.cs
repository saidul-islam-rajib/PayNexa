using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PayNexa.Common.Logging;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.Common.DomainEvents;

internal sealed class DomainEventDispatcher(IServiceProvider services, ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    private delegate Task HandlerInvoker(IServiceProvider services, IDomainEvent domainEvent, CancellationToken cancellationToken);

    private static readonly ConcurrentDictionary<Type, HandlerInvoker> Invokers = new();

    private static readonly MethodInfo InvokeHandlersMethod =
        typeof(DomainEventDispatcher).GetMethod(nameof(InvokeHandlersAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            var eventName = domainEvent.GetType().Name;

            using var step = logger.BeginStep($"Handle domain event {eventName}")
                .WithProperty("DomainEventId", domainEvent.EventId);

            await Invokers.GetOrAdd(domainEvent.GetType(), CreateInvoker)(services, domainEvent, cancellationToken);
            step.Succeeded();
        }
    }

    private static HandlerInvoker CreateInvoker(Type eventType) =>
        InvokeHandlersMethod.MakeGenericMethod(eventType).CreateDelegate<HandlerInvoker>();

    private static async Task InvokeHandlersAsync<TDomainEvent>(IServiceProvider services, IDomainEvent domainEvent, CancellationToken cancellationToken)
        where TDomainEvent : IDomainEvent
    {
        foreach (var handler in services.GetServices<IDomainEventHandler<TDomainEvent>>())
        {
            await handler.HandleAsync((TDomainEvent)domainEvent, cancellationToken);
        }
    }
}
