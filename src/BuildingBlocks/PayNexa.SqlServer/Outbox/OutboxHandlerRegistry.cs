using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PayNexa.Common.Persistence;

namespace PayNexa.SqlServer.Outbox;

public delegate Task OutboxDispatcher(IServiceProvider services, string payload, CancellationToken cancellationToken);

public sealed class OutboxHandlerRegistry
{
    private readonly Dictionary<string, OutboxDispatcher> _dispatchers = new(StringComparer.Ordinal);

    public void Register<TMessage>()
        where TMessage : class =>
        _dispatchers.TryAdd(OutboxSerializer.TypeNameOf(typeof(TMessage)), DispatchAsync<TMessage>);

    public bool TryGetDispatcher(string messageType, [NotNullWhen(true)] out OutboxDispatcher? dispatcher) =>
        _dispatchers.TryGetValue(messageType, out dispatcher);

    private static async Task DispatchAsync<TMessage>(IServiceProvider services, string payload, CancellationToken cancellationToken)
        where TMessage : class
    {
        var message = JsonSerializer.Deserialize<TMessage>(payload, OutboxSerializer.Options)
                      ?? throw new InvalidOperationException($"Outbox payload for {typeof(TMessage).Name} deserialized to null.");

        foreach (var handler in services.GetServices<IOutboxMessageHandler<TMessage>>())
        {
            await handler.HandleAsync(message, cancellationToken);
        }
    }
}
