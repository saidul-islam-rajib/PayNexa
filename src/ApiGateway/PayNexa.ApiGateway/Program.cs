using PayNexa.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPayNexaHealthChecks();

var app = builder.Build();

app.MapPayNexaHealthChecks();

app.Run();
