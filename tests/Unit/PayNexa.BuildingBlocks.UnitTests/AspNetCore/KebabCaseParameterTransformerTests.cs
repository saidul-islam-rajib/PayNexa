using PayNexa.AspNetCore.Conventions;

namespace PayNexa.BuildingBlocks.UnitTests.AspNetCore;

public sealed class KebabCaseParameterTransformerTests
{
    [Theory]
    [InlineData("Customers", "customers")]
    [InlineData("PaymentEligibility", "payment-eligibility")]
    [InlineData("PaymentMethods", "payment-methods")]
    [InlineData("Lookup", "lookup")]
    public void TransformOutbound_ProducesKebabCase(string value, string expected) =>
        new KebabCaseParameterTransformer().TransformOutbound(value).ShouldBe(expected);
}
