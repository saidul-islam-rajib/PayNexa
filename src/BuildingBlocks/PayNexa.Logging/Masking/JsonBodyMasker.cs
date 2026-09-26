using System.Text.Json;
using System.Text.Json.Nodes;

namespace PayNexa.Logging.Masking;

public static class JsonBodyMasker
{
    public static bool TryMask(string json, out string masked)
    {
        try
        {
            var node = JsonNode.Parse(json);
            masked = node is null ? json : MaskNode(node, SensitiveFieldKind.None).ToJsonString();
            return true;
        }
        catch (JsonException)
        {
            masked = string.Empty;
            return false;
        }
    }

    private static JsonNode MaskNode(JsonNode node, SensitiveFieldKind inheritedKind) => node switch
    {
        _ when inheritedKind == SensitiveFieldKind.Secret => JsonValue.Create(SensitiveDataMaskingEnricher.Redacted),
        JsonObject jsonObject => MaskObject(jsonObject),
        JsonArray array => MaskArray(array, inheritedKind),
        JsonValue value => MaskValue(value, inheritedKind),
        _ => node,
    };

    private static JsonObject MaskObject(JsonObject jsonObject)
    {
        foreach (var (name, child) in jsonObject.ToArray())
        {
            if (child is not null)
            {
                jsonObject[name] = MaskNode(child.DeepClone(), SensitiveFields.Classify(name));
            }
        }

        return jsonObject;
    }

    private static JsonArray MaskArray(JsonArray array, SensitiveFieldKind inheritedKind)
    {
        for (var index = 0; index < array.Count; index++)
        {
            if (array[index] is { } element)
            {
                array[index] = MaskNode(element.DeepClone(), inheritedKind);
            }
        }

        return array;
    }

    private static JsonNode MaskValue(JsonValue value, SensitiveFieldKind kind)
    {
        if (kind == SensitiveFieldKind.None)
        {
            return value;
        }

        if (!value.TryGetValue<string>(out var text))
        {
            return JsonValue.Create(SensitiveDataMaskingEnricher.Redacted);
        }

        return JsonValue.Create(SensitiveFields.Mask(kind, text));
    }
}
