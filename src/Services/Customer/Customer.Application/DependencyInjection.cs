using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PayNexa.Common.DomainEvents;
using PayNexa.Customers.Application.Options;

namespace PayNexa.Customers.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddOptions<CustomerOptions>()
            .BindConfiguration(CustomerOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        services.AddDomainEventHandlers(assembly);

        return services;
    }
}
