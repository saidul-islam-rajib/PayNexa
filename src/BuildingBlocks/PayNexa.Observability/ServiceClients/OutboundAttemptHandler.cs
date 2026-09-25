using PayNexa.Common.Correlation;

namespace PayNexa.Observability.ServiceClients;

public sealed class OutboundAttemptHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.RecordAttempt();

        var correlationId = CorrelationContext.Current;
        if (correlationId is not null)
        {
            request.Headers.Remove(CorrelationContext.HeaderName);
            request.Headers.TryAddWithoutValidation(CorrelationContext.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
