namespace PayNexa.Common.Querying;

public sealed record PageRequest(int Page, int PageSize)
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static readonly PageRequest Default = new(DefaultPage, DefaultPageSize);

    public int Skip => (Page - 1) * PageSize;
}
