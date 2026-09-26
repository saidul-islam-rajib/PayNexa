using System.Text.Json;
using System.Text.Json.Serialization;

namespace PayNexa.SqlServer.Outbox;

internal static class OutboxSerializer
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static string TypeNameOf(Type messageType) =>
        messageType.FullName ?? throw new InvalidOperationException(string.Format(OutboxErrorMessages.MissingTypeNameFormat, messageType));

    public static string ShortName(string typeName) => typeName[(typeName.LastIndexOf('.') + 1)..];
}
