using Microsoft.OpenApi;
using PayNexa.AspNetCore.OpenApi;

namespace PayNexa.BuildingBlocks.UnitTests.AspNetCore;

public sealed class OptionalObjectPropertyDocumentTransformerTests
{
    [Fact]
    public async Task TransformAsync_NullableObjectProperty_BecomesOptionalReference()
    {
        var document = new OpenApiDocument();
        var reference = new OpenApiSchemaReference("AddressDto", document);
        var request = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "email", "address" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["email"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["address"] = new OpenApiSchema { OneOf = [new OpenApiSchema { Type = JsonSchemaType.Null }, reference] },
            },
        };
        document.Components = new OpenApiComponents { Schemas = new Dictionary<string, IOpenApiSchema> { ["RegisterCustomerRequest"] = request } };

        await new OptionalObjectPropertyDocumentTransformer().TransformAsync(document, null!, TestContext.Current.CancellationToken);

        request.Properties["address"].ShouldBeSameAs(reference);
        request.Required.ShouldBe(["email"]);
        request.Properties["email"].Type.ShouldBe(JsonSchemaType.String);
    }
}
