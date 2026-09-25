using System.Diagnostics.Metrics;
using System.Net;
using PayNexa.Common.Exceptions;
using PayNexa.Observability.ServiceClients;
using Polly.CircuitBreaker;

namespace PayNexa.BuildingBlocks.UnitTests.ServiceClients;

public sealed class ServiceCallLoggingHandlerTests : IDisposable
{
    private readonly FakeLogger<ServiceCallLoggingHandler> _logger = new();
    private readonly ServiceCallMetrics _metrics = new(new TestMeterFactory());

    [Fact]
    public async Task Success_LogsStartedSucceededEnded()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using var response = await client.SendAsync(Request().WithOperation("GetCustomer"), TestContext.Current.CancellationToken);

        var messages = _logger.Collector.GetSnapshot().Select(record => record.Message).ToArray();
        messages[0].ShouldBe("payment-service -> customer-service GetCustomer started (GET /api/v1/customers/1)");
        messages[1].ShouldContain("succeeded");
        messages[1].ShouldContain("responded 200");
        messages[^1].ShouldBe("payment-service -> customer-service GetCustomer ended");
    }

    [Fact]
    public async Task ServerError_LogsErrorWithStatusCode()
    {
        var client = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        using var response = await client.SendAsync(Request().WithOperation("GetCustomer"), TestContext.Current.CancellationToken);

        var failure = _logger.Collector.GetSnapshot().Single(record => record.Level == LogLevel.Error);
        failure.Message.ShouldContain("responded 503");
    }

    [Fact]
    public async Task OpenCircuit_ThrowsDownstreamServiceExceptionMarkedAsLogged()
    {
        var client = CreateClient(_ => throw new BrokenCircuitException("open"));

        var exception = await Should.ThrowAsync<DownstreamServiceException>(() => client.SendAsync(Request().WithOperation("GetCustomer"), TestContext.Current.CancellationToken));

        exception.FailureKind.ShouldBe(DownstreamFailureKind.CircuitOpen);
        exception.TargetService.ShouldBe("customer-service");
        exception.IsLogged().ShouldBeTrue();
        _logger.Collector.GetSnapshot().ShouldContain(record => record.Level == LogLevel.Error && record.Message.Contains("customer-service is unavailable"));
    }

    [Fact]
    public async Task UnreachableService_ThrowsDownstreamServiceException()
    {
        var client = CreateClient(_ => throw new HttpRequestException("connection refused"));

        var exception = await Should.ThrowAsync<DownstreamServiceException>(() => client.SendAsync(Request(), TestContext.Current.CancellationToken));

        exception.FailureKind.ShouldBe(DownstreamFailureKind.Unreachable);
    }

    public void Dispose() => _metrics.Dispose();

    private HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new ServiceCallLoggingHandler("payment-service", "customer-service", _metrics, _logger)
        {
            InnerHandler = new StubHandler(respond),
        };

        return new HttpClient(handler) { BaseAddress = new Uri("http://customer.api") };
    }

    private static HttpRequestMessage Request() => new(HttpMethod.Get, "/api/v1/customers/1");

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }

    private sealed class TestMeterFactory : IMeterFactory
    {
        private readonly List<Meter> _meters = [];

        public Meter Create(MeterOptions options)
        {
            var meter = new Meter(options);
            _meters.Add(meter);
            return meter;
        }

        public void Dispose() => _meters.ForEach(meter => meter.Dispose());
    }
}
