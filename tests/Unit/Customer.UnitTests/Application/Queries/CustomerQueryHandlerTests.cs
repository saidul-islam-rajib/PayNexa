using Microsoft.Extensions.Options;
using PayNexa.Common.Caching;
using PayNexa.Common.Querying;
using PayNexa.Customers.Application.Caching;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Options;
using PayNexa.Customers.Application.Queries.GetCustomerByEmail;
using PayNexa.Customers.Application.Queries.GetCustomerById;
using PayNexa.Customers.Application.Queries.GetCustomerPaymentEligibility;
using PayNexa.Customers.Application.Queries.ListCustomers;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.Customers.Domain.CustomerAggregate.Enums;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;
using SortDirection = PayNexa.Common.Querying.SortDirection;

namespace PayNexa.Customers.UnitTests.Application.Queries;

public sealed class CustomerQueryHandlerTests
{
    private readonly ICustomerReadStore _readStore = Substitute.For<ICustomerReadStore>();
    private readonly ICustomerRepository _repository = Substitute.For<ICustomerRepository>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly CustomerOptions _options = new() { ProfileCacheTimeToLive = TimeSpan.FromMinutes(3) };

    [Fact]
    public async Task GetById_CacheHit_ReturnsCachedWithoutQueryingReadStore()
    {
        var customer = Response();
        _cache.GetAsync<CustomerResponse>(CustomerCacheKeys.Profile(customer.Id), Arg.Any<CancellationToken>()).Returns(customer);

        var result = await GetById().Handle(new GetCustomerByIdQuery(customer.Id), TestContext.Current.CancellationToken);

        result.Value.ShouldBe(customer);
        await _readStore.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_CacheMiss_ReadsFromReadStoreAndCachesWithConfiguredLifetime()
    {
        var customer = Response();
        _readStore.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var result = await GetById().Handle(new GetCustomerByIdQuery(customer.Id), TestContext.Current.CancellationToken);

        result.Value.ShouldBe(customer);
        await _cache.Received(1).SetAsync(CustomerCacheKeys.Profile(customer.Id), customer, TimeSpan.FromMinutes(3), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFound() =>
        (await GetById().Handle(new GetCustomerByIdQuery(Guid.CreateVersion7()), TestContext.Current.CancellationToken))
            .Error!.Code.ShouldBe(CustomerErrorCodes.NotFound);

    [Fact]
    public async Task GetByEmail_NormalizesBeforeLookingUp()
    {
        var customer = Response();
        _readStore.GetByEmailAsync("rajib@example.com", Arg.Any<CancellationToken>()).Returns(customer);

        var result = await new GetCustomerByEmailQueryHandler(_readStore).Handle(new GetCustomerByEmailQuery("  Rajib@Example.com "), TestContext.Current.CancellationToken);

        result.Value.ShouldBe(customer);
    }

    [Fact]
    public async Task PaymentEligibility_ReadsTheWriteStore()
    {
        var customer = CustomerTestData.RegisteredCustomer(address: CustomerTestData.DhakaAddress());
        customer.VerifyKyc(CustomerTestData.Now);
        _repository.GetByIdAsync(Arg.Any<CustomerId>(), Arg.Any<CancellationToken>()).Returns(customer);

        var result = await new GetCustomerPaymentEligibilityQueryHandler(_repository)
            .Handle(new GetCustomerPaymentEligibilityQuery(customer.Id.Value), TestContext.Current.CancellationToken);

        result.Value.IsEligible.ShouldBeTrue();
        result.Value.Reasons.ShouldBeEmpty();
        await _readStore.DidNotReceiveWithAnyArgs().GetByIdAsync(default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task List_ParsesFiltersAndDefaultSorting()
    {
        _readStore.ListAsync(Arg.Any<CustomerListCriteria>(), Arg.Any<CancellationToken>())
            .Returns(PagedResult<CustomerResponse>.Empty(PageRequest.Default));

        await new ListCustomersQueryHandler(_readStore).Handle(
            new ListCustomersQuery(PageRequest.Default, "  rajib ", "suspended", "verified", null, null),
            TestContext.Current.CancellationToken);

        await _readStore.Received(1).ListAsync(
            new CustomerListCriteria(
                PageRequest.Default,
                "rajib",
                CustomerStatus.Suspended,
                KycStatus.Verified,
                new SortRequest<CustomerSortField>(CustomerSortField.CreatedAt, SortDirection.Descending)),
            Arg.Any<CancellationToken>());
    }

    private GetCustomerByIdQueryHandler GetById() => new(_readStore, _cache, Options.Create(_options));

    private static CustomerResponse Response() => new(
        Guid.CreateVersion7(), "Saidul Islam", "Rajib", "rajib@example.com", "+8801700000000", new DateOnly(1995, 5, 20),
        null, "Active", null, "Pending", null, CustomerTestData.Now, "system", CustomerTestData.Now, "system");
}
