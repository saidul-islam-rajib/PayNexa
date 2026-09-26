using System.Diagnostics.Metrics;

namespace PayNexa.Observability.ServiceClients;

public sealed class ServiceCallMetrics : IDisposable
{
    public const string MeterName = PayNexaTelemetry.SourceNamePrefix + "ServiceCalls";

    private readonly Meter _meter;
    private readonly Histogram<double> _duration;
    private readonly Counter<long> _failures;
    private readonly Counter<long> _retries;
    private readonly Counter<long> _circuitOpened;

    public ServiceCallMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MeterName);
        _duration = _meter.CreateHistogram<double>("paynexa.service_call.duration", "ms", "Duration of service-to-service calls including retries");
        _failures = _meter.CreateCounter<long>("paynexa.service_call.failures", description: "Service-to-service calls that failed after all retries");
        _retries = _meter.CreateCounter<long>("paynexa.service_call.retries", description: "Retry attempts of service-to-service calls");
        _circuitOpened = _meter.CreateCounter<long>("paynexa.service_call.circuit_opened", description: "Circuit breaker transitions to open");
    }

    public void RecordDuration(string targetService, string operation, string outcome, double durationMs) =>
        _duration.Record(durationMs, Tags(targetService, operation, outcome));

    public void RecordFailure(string targetService, string operation, string reason) =>
        _failures.Add(1, Tags(targetService, operation, reason));

    public void RecordRetry(string targetService) =>
        _retries.Add(1, new KeyValuePair<string, object?>("target_service", targetService));

    public void RecordCircuitOpened(string targetService) =>
        _circuitOpened.Add(1, new KeyValuePair<string, object?>("target_service", targetService));

    public void Dispose() => _meter.Dispose();

    private static KeyValuePair<string, object?>[] Tags(string targetService, string operation, string outcome) =>
    [
        new("target_service", targetService),
        new("operation", operation),
        new("outcome", outcome),
    ];
}
