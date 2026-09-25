using PayNexa.AspNetCore;
using PayNexa.Common.Behaviors;
using PayNexa.Customers.Application;
using PayNexa.Customers.Application.Commands.CreateCustomer;
using PayNexa.Customers.Infrastructure;
using PayNexa.Logging;
using Serilog;

Log.Logger = PayNexaLogging.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddPayNexaServiceDefaults();
    builder.Services.AddMediator(options =>
    {
        options.ServiceLifetime = ServiceLifetime.Scoped;
        options.Assemblies = [typeof(CreateCustomerCommand)];
        options.PipelineBehaviors = [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>)];
    });
    builder.Services.AddCustomerApplication();
    builder.AddCustomerInfrastructure();

    var app = builder.Build();

    app.UsePayNexaServiceDefaults();
    await app.ApplyCustomerDatabaseMigrationsAsync();
    await app.RunAsync();

    return 0;
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "Customer service terminated unexpectedly during startup");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
