using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PayNexa.Common.DomainEvents;

public static class DomainEventServiceCollectionExtensions
{
    public static IServiceCollection AddDomainEventHandlers(this IServiceCollection services, Assembly assembly)
    {
        services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        var registrations = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                .Select(contract => (Contract: contract, Implementation: type)));

        foreach (var (contract, implementation) in registrations)
        {
            services.AddScoped(contract, implementation);
        }

        return services;
    }
}
