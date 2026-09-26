using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PayNexa.AspNetCore.OpenApi;

public sealed class OptionalObjectPropertyDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var schema in document.Components?.Schemas?.Values.OfType<OpenApiSchema>() ?? [])
        {
            UnwrapNullableObjectProperties(schema);
        }

        return Task.CompletedTask;
    }

    private static void UnwrapNullableObjectProperties(OpenApiSchema schema)
    {
        if (schema.Properties is null)
        {
            return;
        }

        foreach (var (name, property) in schema.Properties.ToArray())
        {
            if (NonNullAlternative(property) is { } alternative)
            {
                schema.Properties[name] = alternative;
                schema.Required?.Remove(name);
            }
        }
    }

    private static IOpenApiSchema? NonNullAlternative(IOpenApiSchema property)
    {
        if (property.OneOf is not { Count: 2 } alternatives)
        {
            return null;
        }

        var nonNull = alternatives.Where(alternative => alternative.Type != JsonSchemaType.Null).ToArray();
        return nonNull.Length == 1 ? nonNull[0] : null;
    }
}
