using Microsoft.AspNetCore.Mvc;
using PayNexa.Common.Querying;

namespace PayNexa.AspNetCore.Querying;

public abstract class PagedRequest
{
    [FromQuery(Name = QueryParameterNames.Page)]
    public int Page { get; init; } = PageRequest.DefaultPage;

    [FromQuery(Name = QueryParameterNames.PageSize)]
    public int PageSize { get; init; } = PageRequest.DefaultPageSize;

    [FromQuery(Name = QueryParameterNames.Search)]
    public string? Search { get; init; }

    [FromQuery(Name = QueryParameterNames.SortBy)]
    public string? SortBy { get; init; }

    [FromQuery(Name = QueryParameterNames.SortOrder)]
    public string? SortOrder { get; init; }

    public PageRequest ToPageRequest() => new(Page, PageSize);
}
