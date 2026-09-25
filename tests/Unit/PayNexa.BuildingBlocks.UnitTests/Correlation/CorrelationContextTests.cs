using PayNexa.Common.Correlation;

namespace PayNexa.BuildingBlocks.UnitTests.Correlation;

public sealed class CorrelationContextTests
{
    [Theory]
    [InlineData("demo-create-001")]
    [InlineData("01a0d7759f7a7123a0ac3bf6817361c6")]
    [InlineData("COR:10001.retry_2")]
    public void IsValid_AcceptsSafeIdentifiers(string candidate) =>
        CorrelationContext.IsValid(candidate).ShouldBeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("contains space")]
    [InlineData("line\nbreak")]
    [InlineData("<script>")]
    public void IsValid_RejectsUnsafeIdentifiers(string? candidate) =>
        CorrelationContext.IsValid(candidate).ShouldBeFalse();

    [Fact]
    public void IsValid_RejectsIdentifiersLongerThan64Characters() =>
        CorrelationContext.IsValid(new string('a', 65)).ShouldBeFalse();

    [Fact]
    public void Begin_RestoresPreviousValueWhenDisposed()
    {
        using (CorrelationContext.Begin("outer"))
        {
            using (CorrelationContext.Begin("inner"))
            {
                CorrelationContext.Current.ShouldBe("inner");
            }

            CorrelationContext.Current.ShouldBe("outer");
        }

        CorrelationContext.Current.ShouldBeNull();
    }
}
