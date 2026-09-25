namespace PayNexa.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    public Uri? OtlpTracesEndpoint { get; set; }

    public Uri? OtlpMetricsEndpoint { get; set; }

    public string? OtlpHeaders { get; set; }
}
