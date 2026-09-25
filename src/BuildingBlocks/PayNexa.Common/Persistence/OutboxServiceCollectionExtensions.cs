using Microsoft.Extensions.DependencyInjection;

namespace PayNexa.Common.Persistence;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxHandler<TMessage, THandler>(this IServiceCollection services)
        where TMessage : class
        where THandler : class, IOutboxMessageHandler<TMessage> =>
        services.AddScoped<IOutboxMessageHandler<TMessage>, THandler>();
}
