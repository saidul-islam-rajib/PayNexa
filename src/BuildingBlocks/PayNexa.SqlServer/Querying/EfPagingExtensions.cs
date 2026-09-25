using Microsoft.EntityFrameworkCore;
using PayNexa.Common.Querying;

namespace PayNexa.SqlServer.Querying;

public static class EfPagingExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> orderedQuery,
        PageRequest page,
        CancellationToken cancellationToken)
    {
        var totalCount = await orderedQuery.LongCountAsync(cancellationToken);

        if (totalCount == 0 || page.Skip >= totalCount)
        {
            return PagedResult<T>.Create([], page, totalCount);
        }

        var items = await orderedQuery
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, page, totalCount);
    }
}
