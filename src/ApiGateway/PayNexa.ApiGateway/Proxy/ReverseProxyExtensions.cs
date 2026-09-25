using PayNexa.Common.Correlation;
using Yarp.ReverseProxy.Transforms;

namespace PayNexa.ApiGateway.Proxy;

public static class ReverseProxyExtensions
{
    public const string SectionName = "ReverseProxy";

    public static IServiceCollection AddPayNexaReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection(SectionName))
            .AddTransforms(context => context.AddRequestTransform(transform =>
            {
                if (CorrelationContext.Current is { } correlationId)
                {
                    transform.ProxyRequest.Headers.Remove(CorrelationContext.HeaderName);
                    transform.ProxyRequest.Headers.TryAddWithoutValidation(CorrelationContext.HeaderName, correlationId);
                }

                return ValueTask.CompletedTask;
            }));

        return services;
    }
}
