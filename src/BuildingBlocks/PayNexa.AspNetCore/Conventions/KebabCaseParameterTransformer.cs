using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace PayNexa.AspNetCore.Conventions;

public sealed partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value) =>
        value?.ToString() is { Length: > 0 } text
            ? WordBoundary().Replace(text, "$1-$2").ToLowerInvariant()
            : null;

    [GeneratedRegex("([a-z0-9])([A-Z])", RegexOptions.CultureInvariant)]
    private static partial Regex WordBoundary();
}
