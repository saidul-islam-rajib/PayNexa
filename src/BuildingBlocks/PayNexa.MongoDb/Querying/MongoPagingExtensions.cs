using MongoDB.Driver;
using PayNexa.Common.Querying;

namespace PayNexa.MongoDb.Querying;

public static class MongoPagingExtensions
{
    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TDocument, TResult>(
        this IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> filter,
        SortDefinition<TDocument> sort,
        PageRequest page,
        Func<TDocument, TResult> map,
        CancellationToken cancellationToken)
    {
        var totalCount = await collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        if (totalCount == 0 || page.Skip >= totalCount)
        {
            return PagedResult<TResult>.Create([], page, totalCount);
        }

        var documents = await collection
            .Find(filter)
            .Sort(sort)
            .Skip(page.Skip)
            .Limit(page.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<TResult>.Create(documents.Select(map).ToList(), page, totalCount);
    }
}
