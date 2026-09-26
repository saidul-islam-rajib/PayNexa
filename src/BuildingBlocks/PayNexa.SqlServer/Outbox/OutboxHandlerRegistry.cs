using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PayNexa.Common.Persistence;

namespace PayNexa.SqlServer.Outbox;

public delegate Task OutboxDispatcher(IServiceProvider services, string payload, CancellationToken cancellationToken);

public sealed class OutboxHandlerRegistry
{
    private static readonly MethodInfo DispatchMethod =
        typeof(OutboxHandlerRegistry).GetMethod(nameof(DispatchAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly Dictionary<string, OutboxDispatcher> _dispatchers = new(StringComparer.Ordinal);

    public static OutboxHandlerRegistry FromServices(IServiceCollection services)
    {
        var registry = new OutboxHandlerRegistry();

        var messageTypes = services
            .Select(descriptor => descriptor.ServiceType)
            .Where(serviceType => serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IOutboxMessageHandler<>))
            .Select(serviceType => serviceType.GetGenericArguments()[0])
            .Distinct();

        foreach (var messageType in messageTypes)
        {
            registry.Register(messageType);
        }

        return registry;
    }

    public void Register<TMessage>()
        where TMessage : class =>
        Register(typeof(TMessage));

    public bool TryGetDispatcher(string messageType, [NotNullWhen(true)] out OutboxDispatcher? dispatcher) =>
        _dispatchers.TryGetValue(messageType, out dispatcher);

    private void Register(Type messageType) =>
        _dispatchers.TryAdd(
            OutboxSerializer.TypeNameOf(messageType),
            DispatchMethod.MakeGenericMethod(messageType).CreateDelegate<OutboxDispatcher>());

    private static async Task DispatchAsync<TMessage>(IServiceProvider services, string payload, CancellationToken cancellationToken)
        where TMessage : class
    {
        var message = JsonSerializer.Deserialize<TMessage>(payload, OutboxSerializer.Options)
                      ?? throw new InvalidOperationException(string.Format(OutboxErrorMessages.NullPayloadFormat, typeof(TMessage).Name));

        foreach (var handler in services.GetServices<IOutboxMessageHandler<TMessage>>())
        {
            await handler.HandleAsync(message, cancellationToken);
        }
    }
}
