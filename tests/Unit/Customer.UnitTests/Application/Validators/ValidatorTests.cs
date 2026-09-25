using PayNexa.Customers.Application.Commands.CreateCustomer;
using PayNexa.Customers.Application.Commands.UpdateCustomer;
using PayNexa.Customers.Application.Queries.ListCustomers;

namespace PayNexa.Customers.UnitTests.Application.Validators;

public sealed class ValidatorTests
{
    private readonly CreateCustomerCommandValidator _createValidator = new(CustomerTestData.TimeProvider());

    [Fact]
    public async Task CreateCustomer_ValidCommand_Passes()
    {
        var result = await _createValidator.ValidateAsync(ValidCreateCommand(), TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateCustomer_InvalidFields_ReportsEachField()
    {
        var command = new CreateCustomerCommand("", "Rajib", "not-an-email", "017", new DateOnly(2030, 1, 1));

        var result = await _createValidator.ValidateAsync(command, TestContext.Current.CancellationToken);

        result.Errors.Select(error => error.PropertyName).Distinct().ShouldBe(
            ["FirstName", "Email", "PhoneNumber", "DateOfBirth"],
            ignoreOrder: true);
    }

    [Fact]
    public async Task UpdateCustomer_EmptyId_Fails()
    {
        var result = await new UpdateCustomerCommandValidator().ValidateAsync(
            new UpdateCustomerCommand(Guid.Empty, "Saidul", "Rajib", "+8801700000000"),
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Id");
    }

    [Theory]
    [InlineData(0, 20, null, null)]
    [InlineData(1, 0, null, null)]
    [InlineData(1, 101, null, null)]
    [InlineData(1, 20, "password", null)]
    [InlineData(1, 20, null, "sideways")]
    public async Task ListCustomers_InvalidPagingOrSorting_Fails(int page, int pageSize, string? sortBy, string? sortOrder)
    {
        var result = await new ListCustomersQueryValidator().ValidateAsync(
            new ListCustomersQuery(page, pageSize, null, sortBy, sortOrder),
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData("created-at", "desc")]
    [InlineData("LAST-NAME", "ASC")]
    [InlineData(null, null)]
    public async Task ListCustomers_SupportedSorting_Passes(string? sortBy, string? sortOrder)
    {
        var result = await new ListCustomersQueryValidator().ValidateAsync(
            new ListCustomersQuery(1, 20, "rajib", sortBy, sortOrder),
            TestContext.Current.CancellationToken);

        result.IsValid.ShouldBeTrue();
    }

    private static CreateCustomerCommand ValidCreateCommand() =>
        new("Saidul Islam", "Rajib", "saidul.is.rajib@gmail.com", "+8801700000000", new DateOnly(1995, 5, 20));
}
