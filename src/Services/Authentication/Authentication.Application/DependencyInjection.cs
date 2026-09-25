using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PayNexa.Common.DomainEvents;

namespace PayNexa.Authentication.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddDomainEventHandlers(assembly);

        return services;
    }
}
