using MongoDB.Driver;
using PayNexa.MongoDb.Indexes;

namespace PayNexa.Customers.Infrastructure.MongoDB;

internal sealed class CustomerReadModelIndexes : IMongoIndexDefinition
{
    public string CollectionName => CustomerReadModel.CollectionName;

    public Task EnsureIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken)
    {
        var keys = Builders<CustomerReadModel>.IndexKeys;

        return database.GetCollection<CustomerReadModel>(CollectionName).Indexes.CreateManyAsync(
            [
                new CreateIndexModel<CustomerReadModel>(keys.Descending(model => model.CreatedAtUtc), new CreateIndexOptions { Name = "ix_createdAtUtc" }),
                new CreateIndexModel<CustomerReadModel>(keys.Ascending(model => model.LastName), new CreateIndexOptions { Name = "ix_lastName" }),
                new CreateIndexModel<CustomerReadModel>(keys.Ascending(model => model.FirstName), new CreateIndexOptions { Name = "ix_firstName" }),
                new CreateIndexModel<CustomerReadModel>(keys.Ascending(model => model.Email), new CreateIndexOptions { Name = "ix_email" }),
            ],
            cancellationToken);
    }
}
