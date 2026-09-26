using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PayNexa.Common.Correlation;

namespace PayNexa.AspNetCore.ProblemDetails;

public static class ProblemDetailsExtensions
{
    public const string ErrorCodeExtension = "errorCode";
    public const string TraceIdExtension = "traceId";
    public const string CorrelationIdExtension = "correlationId";

    public static IServiceCollection AddPayNexaProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;
            var httpContext = context.HttpContext;

            problem.Instance ??= httpContext.Request.Path;
            problem.Extensions[TraceIdExtension] = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

            var correlationId = CorrelationContext.Current;
            if (correlationId is not null)
            {
                problem.Extensions[CorrelationIdExtension] = correlationId;
            }
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
}
