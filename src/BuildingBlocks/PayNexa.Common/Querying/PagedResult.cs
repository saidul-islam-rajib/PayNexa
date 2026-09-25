namespace PayNexa.Common.Querying;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> Create(IReadOnlyList<T> items, PageRequest page, long totalCount) =>
        new(items, page.Page, page.PageSize, totalCount);

    public static PagedResult<T> Empty(PageRequest page) => new([], page.Page, page.PageSize, 0);

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map) => new(Items.Select(map).ToList(), Page, PageSize, TotalCount);
}
