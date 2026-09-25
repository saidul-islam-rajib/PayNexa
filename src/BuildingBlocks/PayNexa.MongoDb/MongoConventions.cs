using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace PayNexa.MongoDb;

internal static class MongoConventions
{
    private static readonly Lazy<bool> Registration = new(Register);

    public static void EnsureRegistered() => _ = Registration.Value;

    private static bool Register()
    {
        ConventionRegistry.Register(
            "PayNexa",
            new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
            },
            _ => true);

        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        return true;
    }
}
