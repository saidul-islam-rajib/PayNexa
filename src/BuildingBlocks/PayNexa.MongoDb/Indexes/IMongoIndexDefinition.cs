using MongoDB.Driver;

namespace PayNexa.MongoDb.Indexes;

public interface IMongoIndexDefinition
{
    string CollectionName { get; }

    Task EnsureIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken);
}
