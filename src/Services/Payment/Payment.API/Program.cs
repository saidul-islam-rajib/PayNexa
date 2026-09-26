using PayNexa.AspNetCore.Initialization;
using PayNexa.Logging;
using PayNexa.Payments.API;
using PayNexa.Payments.Application;
using PayNexa.Payments.Infrastructure;
using Serilog;

Log.Logger = PayNexaLogging.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    {
        builder.Services
            .AddPresentation(builder.Configuration, builder.Environment)
            .AddApplication()
            .AddInfrastructure(builder.Configuration);
    }

    var app = builder.Build();
    {
        app.UsePresentation();
        await app.InitializeInfrastructureAsync();
        await app.RunAsync();
    }

    return 0;
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "Service terminated unexpectedly during startup");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
