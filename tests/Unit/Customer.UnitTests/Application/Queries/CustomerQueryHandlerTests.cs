using PayNexa.Common.Caching;
using PayNexa.Common.Results;
using PayNexa.Customers.Application.Caching;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Queries.GetCustomerById;
using PayNexa.Customers.Application.Queries.ListCustomers;
using PayNexa.Customers.Contracts.Responses;
using SortDirection = PayNexa.Common.Querying.SortDirection;

namespace PayNexa.Customers.UnitTests.Application.Queries;

public sealed class CustomerQueryHandlerTests
{
    private readonly ICustomerReadStore _readStore = Substitute.For<ICustomerReadStore>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    [Fact]
    public async Task GetById_CacheHit_ReturnsCachedWithoutQueryingReadStore()
    {
        var customer = Response();
        _cache.GetAsync<CustomerResponse>(CustomerCacheKeys.Profile(customer.Id), Arg.Any<CancellationToken>()).Returns(customer);

        var result = await new GetCustomerByIdQueryHandler(_readStore, _cache).Handle(new GetCustomerByIdQuery(customer.Id), TestContext.Current.CancellationToken);

        result.Value.ShouldBe(customer);
        await _readStore.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CacheMiss_ReadsFromReadStoreAndPopulatesCache()
    {
        var customer = Response();
        _readStore.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var result = await new GetCustomerByIdQueryHandler(_readStore, _cache).Handle(new GetCustomerByIdQuery(customer.Id), TestContext.Current.CancellationToken);

        result.Value.ShouldBe(customer);
        await _cache.Received(1).SetAsync(CustomerCacheKeys.Profile(customer.Id), customer, CustomerCacheKeys.ProfileTimeToLive, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFoundAndDoesNotCache()
    {
        var result = await new GetCustomerByIdQueryHandler(_readStore, _cache).Handle(new GetCustomerByIdQuery(Guid.CreateVersion7()), TestContext.Current.CancellationToken);

        result.Error!.Code.ShouldBe("Customer.NotFound");
        await _cache.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<CustomerResponse>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task List_DefaultSorting_UsesCreatedAtDescending()
    {
        _readStore.ListAsync(Arg.Any<CustomerListCriteria>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<CustomerResponse>([], 1, 20, 0));

        await new ListCustomersQueryHandler(_readStore).Handle(new ListCustomersQuery(1, 20, "  rajib ", null, null), TestContext.Current.CancellationToken);

        await _readStore.Received(1).ListAsync(
            new CustomerListCriteria(1, 20, "rajib", CustomerSortField.CreatedAt, SortDirection.Descending),
            Arg.Any<CancellationToken>());
    }

    private static CustomerResponse Response() => new(
        Guid.CreateVersion7(), "Saidul Islam", "Rajib", "rajib@example.com", "+8801700000000",
        new DateOnly(1995, 5, 20), "Active", CustomerTestData.Now, CustomerTestData.Now);
}
