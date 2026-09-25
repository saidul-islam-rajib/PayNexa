using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Routing;

namespace PayNexa.AspNetCore.Conventions;

public sealed class ProblemDetailsResponseConvention : IActionModelConvention
{
    public void Apply(ActionModel action)
    {
        var httpMethod = action.Attributes.OfType<HttpMethodAttribute>().SelectMany(attribute => attribute.HttpMethods).FirstOrDefault();

        if (httpMethod is null)
        {
            return;
        }

        var targetsExistingResource = action.Selectors
            .Select(selector => selector.AttributeRouteModel?.Template)
            .Any(template => template?.Contains('{') == true);

        var successType = SuccessTypeOf(action.ActionMethod.ReturnType);

        foreach (var (statusCode, type) in ResponsesFor(httpMethod, targetsExistingResource, successType))
        {
            AddIfMissing(action, statusCode, type);
        }
    }

    private static IEnumerable<(int StatusCode, Type Type)> ResponsesFor(string httpMethod, bool targetsExistingResource, Type successType)
    {
        var isQuery = HttpMethods.IsGet(httpMethod) || HttpMethods.IsHead(httpMethod);

        yield return (SuccessStatusFor(httpMethod, targetsExistingResource), successType);
        yield return (StatusCodes.Status400BadRequest, typeof(ValidationProblemDetails));

        if (targetsExistingResource)
        {
            yield return (StatusCodes.Status404NotFound, typeof(Microsoft.AspNetCore.Mvc.ProblemDetails));
        }

        if (!isQuery)
        {
            yield return (StatusCodes.Status409Conflict, typeof(Microsoft.AspNetCore.Mvc.ProblemDetails));
            yield return (StatusCodes.Status422UnprocessableEntity, typeof(Microsoft.AspNetCore.Mvc.ProblemDetails));
        }

        yield return (StatusCodes.Status500InternalServerError, typeof(Microsoft.AspNetCore.Mvc.ProblemDetails));
    }

    private static int SuccessStatusFor(string httpMethod, bool targetsExistingResource) => httpMethod switch
    {
        _ when HttpMethods.IsPost(httpMethod) && !targetsExistingResource => StatusCodes.Status201Created,
        _ when HttpMethods.IsDelete(httpMethod) => StatusCodes.Status204NoContent,
        _ => StatusCodes.Status200OK,
    };

    private static Type SuccessTypeOf(Type returnType)
    {
        var type = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)
            ? returnType.GetGenericArguments()[0]
            : returnType;

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ActionResult<>)
            ? type.GetGenericArguments()[0]
            : typeof(void);
    }

    private static void AddIfMissing(ActionModel action, int statusCode, Type type)
    {
        var alreadyDeclared = action.Filters.OfType<IApiResponseMetadataProvider>().Any(provider => provider.StatusCode == statusCode);

        if (!alreadyDeclared)
        {
            action.Filters.Add(new ProducesResponseTypeAttribute(type, statusCode));
        }
    }
}
