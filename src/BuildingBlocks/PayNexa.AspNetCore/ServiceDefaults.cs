using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PayNexa.AspNetCore.ProblemDetails;
using PayNexa.Common.Behaviors;
using PayNexa.Logging;
using PayNexa.Observability;
using Scalar.AspNetCore;

namespace PayNexa.AspNetCore;

public static class ServiceDefaults
{
    public static WebApplicationBuilder AddPayNexaServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.AddPayNexaLogging();
        builder.AddPayNexaObservability();

        builder.Services.AddOptions<OperationLoggingOptions>()
            .BindConfiguration(OperationLoggingOptions.SectionName)
            .ValidateOnStart();

        builder.Services.AddPayNexaProblemDetails();
        builder.Services.AddRouting(options => options.LowercaseUrls = true);
        builder.Services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            })
            .AddOpenApi();

        return builder;
    }

    public static WebApplication UsePayNexaServiceDefaults(this WebApplication app)
    {
        app.UsePayNexaRequestLogging();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseAuthorization();

        app.MapControllers();
        app.MapPayNexaHealthChecks();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().WithDocumentPerVersion();
            app.MapScalarApiReference();
        }

        return app;
    }
}
