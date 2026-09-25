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
                Index(keys.Descending(model => model.CreatedAtUtc), "ix_createdAtUtc"),
                Index(keys.Descending(model => model.UpdatedAtUtc), "ix_updatedAtUtc"),
                Index(keys.Ascending(model => model.LastName), "ix_lastName"),
                Index(keys.Ascending(model => model.FirstName), "ix_firstName"),
                Index(keys.Ascending(model => model.Email), "ix_email"),
                Index(keys.Ascending(model => model.Status).Ascending(model => model.KycStatus), "ix_status_kycStatus"),
            ],
            cancellationToken);
    }

    private static CreateIndexModel<CustomerReadModel> Index(IndexKeysDefinition<CustomerReadModel> keys, string name) =>
        new(keys, new CreateIndexOptions { Name = name });
}
