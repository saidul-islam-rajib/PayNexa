using PayNexa.ApiGateway.Proxy;
using PayNexa.Logging;
using PayNexa.Observability;
using Serilog;

Log.Logger = PayNexaLogging.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    {
        builder.Services
            .AddPayNexaLogging(builder.Configuration, builder.Environment)
            .AddPayNexaObservability(builder.Configuration, builder.Environment)
            .AddPayNexaReverseProxy(builder.Configuration);
    }

    var app = builder.Build();
    {
        app.UsePayNexaRequestLogging();
        app.MapPayNexaHealthChecks();
        app.MapReverseProxy();
        await app.RunAsync();
    }

    return 0;
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "API gateway terminated unexpectedly during startup");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
