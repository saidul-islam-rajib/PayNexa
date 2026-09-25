using PayNexa.AspNetCore;
using PayNexa.Common.Behaviors;

namespace PayNexa.Authentication.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPayNexaServiceDefaults(configuration, environment);

        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>), typeof(DomainRuleBehavior<,>)];
        });

        return services;
    }

    public static WebApplication UsePresentation(this WebApplication app) => app.UsePayNexaServiceDefaults();
}
