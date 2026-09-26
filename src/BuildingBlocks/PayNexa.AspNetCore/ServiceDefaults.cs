using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PayNexa.AspNetCore.Conventions;
using PayNexa.AspNetCore.OpenApi;
using PayNexa.AspNetCore.ProblemDetails;
using PayNexa.AspNetCore.Security;
using PayNexa.Common.Behaviors;
using PayNexa.Common.Security;
using PayNexa.Logging;
using PayNexa.Observability;

namespace PayNexa.AspNetCore;

public static class ServiceDefaults
{
    public const string SwaggerRoutePrefix = "swagger";
    public const string OpenApiDocumentRoute = "/openapi/v1.json";

    public static IServiceCollection AddPayNexaServiceDefaults(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services
            .AddPayNexaLogging(configuration, environment)
            .AddPayNexaObservability(configuration, environment)
            .AddPayNexaProblemDetails();

        services.AddOptions<OperationLoggingOptions>()
            .BindConfiguration(OperationLoggingOptions.SectionName)
            .ValidateOnStart();

        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentActor, HttpContextCurrentActor>();

        services
            .AddControllers(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
                options.Conventions.Add(new ProblemDetailsResponseConvention());
            })
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc(options => options.Conventions.Add(new VersionByNamespaceConvention()))
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            })
            .AddOpenApi();

        services.ConfigureAll<OpenApiOptions>(options => options.AddDocumentTransformer<OptionalObjectPropertyDocumentTransformer>());

        return services;
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
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = SwaggerRoutePrefix;
                options.SwaggerEndpoint(OpenApiDocumentRoute, app.Environment.ApplicationName);
                options.DisplayRequestDuration();
            });
        }

        return app;
    }
}
