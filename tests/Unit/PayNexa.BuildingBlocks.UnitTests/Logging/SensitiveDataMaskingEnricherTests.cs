using PayNexa.Logging.Masking;
using Serilog.Events;
using Serilog.Parsing;

namespace PayNexa.BuildingBlocks.UnitTests.Logging;

public sealed class SensitiveDataMaskingEnricherTests
{
    private readonly SensitiveDataMaskingEnricher _enricher = new();

    [Theory]
    [InlineData("Password")]
    [InlineData("RefreshToken")]
    [InlineData("ApiKey")]
    [InlineData("CardNumber")]
    [InlineData("Cvv")]
    [InlineData("ConnectionString")]
    public void Enrich_SecretProperty_IsRedacted(string propertyName)
    {
        var logEvent = CreateEvent(new LogEventProperty(propertyName, new ScalarValue("super-secret")));

        _enricher.Enrich(logEvent, null!);

        ScalarText(logEvent, propertyName).ShouldBe(SensitiveDataMaskingEnricher.Redacted);
    }

    [Theory]
    [InlineData("SpanId")]
    [InlineData("CompanyName")]
    [InlineData("AccessTokenExpiresAtUtc")]
    public void Enrich_NonSecretProperty_IsUnchanged(string propertyName)
    {
        var logEvent = CreateEvent(new LogEventProperty(propertyName, new ScalarValue("visible")));

        _enricher.Enrich(logEvent, null!);

        ScalarText(logEvent, propertyName).ShouldBe("visible");
    }

    [Fact]
    public void Enrich_EmailAndPhone_ArePartiallyMasked()
    {
        var logEvent = CreateEvent(
            new LogEventProperty("Email", new ScalarValue("rajib@example.com")),
            new LogEventProperty("PhoneNumber", new ScalarValue("+8801700001234")));

        _enricher.Enrich(logEvent, null!);

        ScalarText(logEvent, "Email").ShouldBe("r***@example.com");
        ScalarText(logEvent, "PhoneNumber").ShouldBe("*********1234");
    }

    [Fact]
    public void Enrich_NestedStructure_MasksInnerSecrets()
    {
        var request = new StructureValue(
        [
            new LogEventProperty("Username", new ScalarValue("rajib")),
            new LogEventProperty("Password", new ScalarValue("Test@123")),
        ]);
        var logEvent = CreateEvent(new LogEventProperty("Request", request));

        _enricher.Enrich(logEvent, null!);

        var masked = (StructureValue)logEvent.Properties["Request"];
        masked.Properties.Single(property => property.Name == "Password").Value.ToString().ShouldContain(SensitiveDataMaskingEnricher.Redacted);
        masked.Properties.Single(property => property.Name == "Username").Value.ToString().ShouldContain("rajib");
    }

    private static LogEvent CreateEvent(params LogEventProperty[] properties) =>
        new(DateTimeOffset.UtcNow, LogEventLevel.Information, null, new MessageTemplate("test", Array.Empty<MessageTemplateToken>()), properties);

    private static string? ScalarText(LogEvent logEvent, string propertyName) =>
        ((ScalarValue)logEvent.Properties[propertyName]).Value?.ToString();
}
