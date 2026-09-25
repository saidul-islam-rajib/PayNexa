using Microsoft.Extensions.Options;
using PayNexa.Common.Querying;
using PayNexa.Customers.Application.Commands.RegisterCustomer;
using PayNexa.Customers.Application.Commands.SuspendCustomer;
using PayNexa.Customers.Application.Options;
using PayNexa.Customers.Application.Queries.ListCustomers;
using PayNexa.Customers.Contracts.Common;

namespace PayNexa.Customers.UnitTests.Application.Validators;

public sealed class ValidatorTests
{
    private readonly RegisterCustomerCommandValidator _registerValidator =
        new(CustomerTestData.TimeProvider(), Options.Create(new CustomerOptions()));

    [Fact]
    public async Task Register_ValidCommand_Passes() =>
        (await _registerValidator.ValidateAsync(Register(), TestContext.Current.CancellationToken)).IsValid.ShouldBeTrue();

    [Fact]
    public async Task Register_InvalidFields_ReportsEachField()
    {
        var result = await _registerValidator.ValidateAsync(
            Register() with { FirstName = "", Email = "not-an-email", PhoneNumber = "017", DateOfBirth = new DateOnly(2030, 1, 1) },
            TestContext.Current.CancellationToken);

        result.Errors.Select(error => error.PropertyName).Distinct().ShouldBe(["FirstName", "Email", "PhoneNumber", "DateOfBirth"], ignoreOrder: true);
    }

    [Fact]
    public async Task Register_Minor_FailsMinimumAge()
    {
        var result = await _registerValidator.ValidateAsync(Register() with { DateOfBirth = new DateOnly(2015, 1, 1) }, TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().ErrorMessage.ShouldContain("18");
    }

    [Fact]
    public async Task Register_InvalidAddress_ReportsNestedFields()
    {
        var result = await _registerValidator.ValidateAsync(
            Register() with { Address = new AddressDto("", null, "Dhaka", null, "1207", "Bangladesh") },
            TestContext.Current.CancellationToken);

        result.Errors.Select(error => error.PropertyName).ShouldBe(["Address.Line1", "Address.CountryCode"], ignoreOrder: true);
    }

    [Fact]
    public async Task Suspend_RequiresReason()
    {
        var result = await new SuspendCustomerCommandValidator().ValidateAsync(new SuspendCustomerCommand(Guid.CreateVersion7(), " "), TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Reason");
    }

    [Theory]
    [InlineData(0, 20, null, null, null, QueryParameterNames.Page)]
    [InlineData(1, 101, null, null, null, QueryParameterNames.PageSize)]
    [InlineData(1, 20, "password", null, null, QueryParameterNames.SortBy)]
    [InlineData(1, 20, null, "sideways", null, QueryParameterNames.SortOrder)]
    [InlineData(1, 20, null, null, "Deleted", CustomerQueryParameterNames.Status)]
    public async Task List_InvalidParameters_ReportQueryParameterName(int page, int pageSize, string? sortBy, string? sortOrder, string? status, string expectedField)
    {
        var result = await new ListCustomersQueryValidator().ValidateAsync(
            new ListCustomersQuery(new PageRequest(page, pageSize), null, status, null, sortBy, sortOrder),
            TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(expectedField);
    }

    [Fact]
    public async Task List_SupportedFiltersAreCaseInsensitive() =>
        (await new ListCustomersQueryValidator().ValidateAsync(
            new ListCustomersQuery(PageRequest.Default, "rajib", "suspended", "VERIFIED", "UPDATED-AT", "asc"),
            TestContext.Current.CancellationToken)).IsValid.ShouldBeTrue();

    private static RegisterCustomerCommand Register() => new(
        "Saidul Islam", "Rajib", "saidul.is.rajib@gmail.com", "+8801700000000", new DateOnly(1995, 5, 20),
        new AddressDto("Mirpur Road", null, "Dhaka", "Dhaka", "1207", "BD"));
}
