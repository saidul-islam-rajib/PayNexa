namespace PayNexa.Observability.ServiceClients;

public static class ServiceCallContext
{
    private static readonly HttpRequestOptionsKey<string> OperationKey = new("PayNexa.ServiceCall.Operation");
    private static readonly HttpRequestOptionsKey<AttemptCounter> AttemptsKey = new("PayNexa.ServiceCall.Attempts");

    public static HttpRequestMessage WithOperation(this HttpRequestMessage request, string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        request.Options.Set(OperationKey, operation);
        return request;
    }

    public static string GetOperation(this HttpRequestMessage request) =>
        request.Options.TryGetValue(OperationKey, out var operation)
            ? operation
            : $"{request.Method.Method} {request.RequestUri?.AbsolutePath}";

    internal static AttemptCounter StartAttemptCounting(this HttpRequestMessage request)
    {
        var counter = new AttemptCounter();
        request.Options.Set(AttemptsKey, counter);
        return counter;
    }

    internal static void RecordAttempt(this HttpRequestMessage request)
    {
        if (request.Options.TryGetValue(AttemptsKey, out var counter))
        {
            counter.Increment();
        }
    }

    internal sealed class AttemptCounter
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public void Increment() => Interlocked.Increment(ref _count);
    }
}
