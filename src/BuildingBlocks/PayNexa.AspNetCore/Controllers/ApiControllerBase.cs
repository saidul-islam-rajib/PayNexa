using System.Text.Json;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PayNexa.AspNetCore.ProblemDetails;
using PayNexa.SharedKernel.Results;

namespace PayNexa.AspNetCore.Controllers;

[ApiController]
[Route(ApiRoutes.Resource)]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? sender;

    protected ISender Sender => sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected async Task<ActionResult<T>> QueryAsync<T>(IQuery<Result<T>> query, CancellationToken cancellationToken) =>
        Respond(await Sender.Send(query, cancellationToken));

    protected async Task<ActionResult<T>> CommandAsync<T>(ICommand<Result<T>> command, CancellationToken cancellationToken) =>
        Respond(await Sender.Send(command, cancellationToken));

    protected async Task<ActionResult<T>> CreateAsync<T>(
        ICommand<Result<T>> command,
        string locationAction,
        Func<T, Guid> locationId,
        CancellationToken cancellationToken) =>
        RespondCreated(await Sender.Send(command, cancellationToken), locationAction, locationId);

    protected ActionResult<T> Respond<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : Problem(result.Error);

    protected ActionResult<T> RespondCreated<T>(Result<T> result, string locationAction, Func<T, Guid> locationId)
    {
        if (result.IsFailure)
        {
            return Problem(result.Error);
        }

        var routeValues = new RouteValueDictionary
        {
            [ApiRoutes.Parameters.Id] = locationId(result.Value),
            [ApiRoutes.Parameters.Version] = RouteData.Values[ApiRoutes.Parameters.Version],
        };

        return CreatedAtAction(locationAction, routeValues, result.Value);
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
