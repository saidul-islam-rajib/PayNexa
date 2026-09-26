using MongoDB.Driver;

namespace PayNexa.MongoDb.ReadModels;

public static class ReadModelCollectionExtensions
{
    public static async Task<bool> UpsertIfNewerAsync<TReadModel>(
        this IMongoCollection<TReadModel> collection,
        TReadModel readModel,
        CancellationToken cancellationToken = default)
        where TReadModel : IVersionedReadModel
    {
        var filter = Builders<TReadModel>.Filter.And(
            Builders<TReadModel>.Filter.Eq(model => model.Id, readModel.Id),
            Builders<TReadModel>.Filter.Lt(model => model.Version, readModel.Version));

        try
        {
            await collection.ReplaceOneAsync(filter, readModel, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            return true;
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }
}
