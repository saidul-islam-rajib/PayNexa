using PayNexa.Logging;
using PayNexa.Observability;
using Serilog;

Log.Logger = PayNexaLogging.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddPayNexaLogging();
    builder.AddPayNexaObservability();

    var app = builder.Build();

    app.UsePayNexaRequestLogging();
    app.MapPayNexaHealthChecks();

    await app.RunAsync();

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
