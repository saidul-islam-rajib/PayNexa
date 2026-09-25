using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace PayNexa.Customers.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerApplication(this IServiceCollection services) =>
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
}
