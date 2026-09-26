using System.Text;
using Microsoft.AspNetCore.Http;
using PayNexa.Logging.RequestLogging;

namespace PayNexa.BuildingBlocks.UnitTests.Logging;

public sealed class HttpBodyLoggingMiddlewareTests
{
    private const string RequestJson = """{"email":"rajib@example.com","password":"Test@123"}""";
    private const string ResponseJson = """{"id":"1","firstName":"Saidul"}""";

    private readonly FakeLogger<HttpBodyLoggingMiddleware> _logger = new();

    [Fact]
    public async Task InvokeAsync_JsonRequestAndResponse_LogsMaskedBodiesAndPassesThemThrough()
    {
        var context = CreateContext("/api/v1/customers", RequestJson);
        var responseSink = (MemoryStream)context.Response.Body;
        string? bodySeenByEndpoint = null;

        var middleware = CreateMiddleware(async httpContext =>
        {
            bodySeenByEndpoint = await new StreamReader(httpContext.Request.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
            httpContext.Response.StatusCode = StatusCodes.Status201Created;
            httpContext.Response.ContentType = "application/json; charset=utf-8";
            await httpContext.Response.WriteAsync(ResponseJson, TestContext.Current.CancellationToken);
        });

        await middleware.InvokeAsync(context);

        bodySeenByEndpoint.ShouldBe(RequestJson);
        Encoding.UTF8.GetString(responseSink.ToArray()).ShouldBe(ResponseJson);

        var messages = _logger.Collector.GetSnapshot().Select(record => record.Message).ToArray();
        messages.ShouldContain("""HTTP POST /api/v1/customers request body: {"email":"r***@example.com","password":"***REDACTED***"}""");
        messages.ShouldContain("""HTTP POST /api/v1/customers response body (201): {"id":"1","firstName":"S***"}""");
    }

    [Fact]
    public async Task InvokeAsync_LongResponse_IsTruncated()
    {
        var context = CreateContext("/api/v1/customers", body: null);
        var longJson = $$"""{"value":"{{new string('x', 100)}}"}""";

        var middleware = CreateMiddleware(async httpContext =>
        {
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(longJson, TestContext.Current.CancellationToken);
        }, new HttpBodyLoggingOptions { Enabled = true, MaxLoggedLength = 20 });

        await middleware.InvokeAsync(context);

        _logger.LatestRecord.Message.ShouldEndWith($"... (truncated, {longJson.Length} chars)");
    }

    [Fact]
    public async Task InvokeAsync_HealthEndpoint_IsNotLogged()
    {
        var context = CreateContext("/health/ready", body: null);

        await CreateMiddleware(httpContext => httpContext.Response.WriteAsync("Healthy")).InvokeAsync(context);

        _logger.Collector.Count.ShouldBe(0);
    }

    private HttpBodyLoggingMiddleware CreateMiddleware(RequestDelegate next, HttpBodyLoggingOptions? options = null) =>
        new(next, _logger, options ?? new HttpBodyLoggingOptions { Enabled = true });

    private static DefaultHttpContext CreateContext(string path, string? body)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = body is null ? HttpMethods.Get : HttpMethods.Post;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();

        if (body is not null)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            context.Request.ContentType = "application/json";
            context.Request.ContentLength = bytes.Length;
            context.Request.Body = new MemoryStream(bytes);
        }

        return context;
    }
}
