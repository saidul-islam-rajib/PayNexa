using PayNexa.Common.Results;

namespace PayNexa.BuildingBlocks.UnitTests.Results;

public sealed class ResultTests
{
    private static readonly Error SampleError = Error.NotFound("Sample.NotFound", "Not found.");

    [Fact]
    public void Success_WithValue_ExposesValue()
    {
        Result<int> result = 42;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_FromError_ExposesError()
    {
        Result<int> result = SampleError;

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(SampleError);
    }

    [Fact]
    public void Value_OnFailedResult_Throws()
    {
        Result<int> result = SampleError;

        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Validation_CarriesFieldErrors()
    {
        var error = Error.Validation(new Dictionary<string, string[]> { ["Email"] = ["Invalid"] });

        error.Type.ShouldBe(ErrorType.Validation);
        error.ValidationErrors!["Email"].ShouldBe(["Invalid"]);
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    public void PagedResult_TotalPages_RoundsUp(long totalCount, int pageSize, int expectedPages)
    {
        var page = new PagedResult<int>([], 1, pageSize, totalCount);

        page.TotalPages.ShouldBe(expectedPages);
    }
}
