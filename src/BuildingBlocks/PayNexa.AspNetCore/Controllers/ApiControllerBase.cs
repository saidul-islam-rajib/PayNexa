using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PayNexa.AspNetCore.ProblemDetails;
using PayNexa.Common.Results;

namespace PayNexa.AspNetCore.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Problem(Error error)
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

    private IActionResult ValidationProblem(Error error)
    {
        var modelState = new ModelStateDictionary();

        foreach (var (field, messages) in error.ValidationErrors!)
        {
            foreach (var message in messages)
            {
                modelState.AddModelError(JsonNamingPolicy.CamelCase.ConvertName(field), message);
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
