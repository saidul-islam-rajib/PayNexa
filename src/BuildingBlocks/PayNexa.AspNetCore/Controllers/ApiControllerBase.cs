using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using PayNexa.AspNetCore.ProblemDetails;
using PayNexa.SharedKernel.Results;

namespace PayNexa.AspNetCore.Controllers;

[ApiController]
[Route(RouteTemplate)]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    public const string RouteTemplate = "api/v{version:apiVersion}/[controller]";
    private const string VersionRouteValue = "version";

    protected ActionResult<T> Respond<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : Problem(result.Error);

    protected ActionResult<T> RespondCreated<T>(Result<T> result, string actionName, Func<T, object> routeValues)
    {
        if (result.IsFailure)
        {
            return Problem(result.Error);
        }

        var values = new RouteValueDictionary(routeValues(result.Value))
        {
            [VersionRouteValue] = RouteData.Values[VersionRouteValue],
        };

        return CreatedAtAction(actionName, values, result.Value);
    }

    protected ActionResult Problem(Error error)
    {
        if (error.Type == ErrorType.Validation && error.ValidationErrors is not null)
        {
            return ValidationProblem(error);
        }

        var statusCode = ErrorHttpMapping.StatusCodeFor(error.Type);
        var problem = ProblemDetailsFactory.CreateProblemDetails(
            HttpContext,
            statusCode,
            ErrorHttpMapping.TitleFor(error.Type),
            detail: error.Description);

        problem.Extensions[ProblemDetailsExtensions.ErrorCodeExtension] = error.Code;
        return new ObjectResult(problem) { StatusCode = statusCode };
    }

    private ActionResult ValidationProblem(Error error)
    {
        var modelState = new ModelStateDictionary();

        foreach (var (field, messages) in error.ValidationErrors!)
        {
            foreach (var message in messages)
            {
                modelState.AddModelError(field.Contains('-') ? field : JsonNamingPolicy.CamelCase.ConvertName(field), message);
            }
        }

        var problem = ProblemDetailsFactory.CreateValidationProblemDetails(
            HttpContext,
            modelState,
            ErrorHttpMapping.StatusCodeFor(error.Type),
            ErrorHttpMapping.TitleFor(error.Type),
            detail: error.Description);

        problem.Extensions[ProblemDetailsExtensions.ErrorCodeExtension] = error.Code;
        return new ObjectResult(problem) { StatusCode = problem.Status };
    }
}
