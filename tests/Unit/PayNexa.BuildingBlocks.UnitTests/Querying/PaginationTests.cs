using PayNexa.Common.Querying;
using SortDirection = PayNexa.Common.Querying.SortDirection;

namespace PayNexa.BuildingBlocks.UnitTests.Querying;

public sealed class PaginationTests
{
    private enum SampleField
    {
        CreatedAt,
        Name,
    }

    private static readonly SortFieldMap<SampleField> Map = new(
        new Dictionary<string, SampleField> { ["created-at"] = SampleField.CreatedAt, ["name"] = SampleField.Name },
        new SortRequest<SampleField>(SampleField.CreatedAt, SortDirection.Descending));

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(3, 20, 40)]
    public void PageRequest_Skip_IsDerivedFromPageAndSize(int page, int pageSize, int expectedSkip) =>
        new PageRequest(page, pageSize).Skip.ShouldBe(expectedSkip);

    [Theory]
    [InlineData(1, 45, false, true)]
    [InlineData(2, 45, true, true)]
    [InlineData(3, 45, true, false)]
    public void PagedResult_NavigationFlags(int page, long totalCount, bool hasPrevious, bool hasNext)
    {
        var result = PagedResult<int>.Create([], new PageRequest(page, 20), totalCount);

        result.TotalPages.ShouldBe(3);
        result.HasPreviousPage.ShouldBe(hasPrevious);
        result.HasNextPage.ShouldBe(hasNext);
    }

    [Fact]
    public void PagedResult_Map_KeepsPagingMetadata()
    {
        var mapped = PagedResult<int>.Create([1, 2], new PageRequest(2, 2), 10).Map(value => value.ToString());

        mapped.Items.ShouldBe(["1", "2"]);
        mapped.Page.ShouldBe(2);
        mapped.TotalCount.ShouldBe(10);
    }

    [Fact]
    public void SortFieldMap_Resolve_UsesDefaultsWhenOmitted() =>
        Map.Resolve(null, null).ShouldBe(new SortRequest<SampleField>(SampleField.CreatedAt, SortDirection.Descending));

    [Fact]
    public void SortFieldMap_Resolve_IsCaseInsensitive() =>
        Map.Resolve("NAME", "ASC").ShouldBe(new SortRequest<SampleField>(SampleField.Name, SortDirection.Ascending));

    [Theory]
    [InlineData("name", true)]
    [InlineData(null, true)]
    [InlineData("password", false)]
    public void SortFieldMap_IsValidField(string? sortBy, bool expected) => Map.IsValidField(sortBy).ShouldBe(expected);

    [Theory]
    [InlineData("asc", true)]
    [InlineData("DESC", true)]
    [InlineData("sideways", false)]
    public void SortFieldMap_IsValidDirection(string sortOrder, bool expected) =>
        SortFieldMap<SampleField>.IsValidDirection(sortOrder).ShouldBe(expected);
}
