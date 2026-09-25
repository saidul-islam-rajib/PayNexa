using Mediator;
using PayNexa.Common.Caching;
using PayNexa.Common.Results;
using PayNexa.Customers.Application.Caching;
using PayNexa.Customers.Application.Errors;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Contracts.Responses;

namespace PayNexa.Customers.Application.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler(ICustomerReadStore readStore, ICacheService cache)
    : IQueryHandler<GetCustomerByIdQuery, Result<CustomerResponse>>
{
    public async ValueTask<Result<CustomerResponse>> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = CustomerCacheKeys.Profile(query.Id);

        var cached = await cache.GetAsync<CustomerResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var customer = await readStore.GetByIdAsync(query.Id, cancellationToken);
        if (customer is null)
        {
            return CustomerErrors.NotFound(query.Id);
        }

        await cache.SetAsync(cacheKey, customer, CustomerCacheKeys.ProfileTimeToLive, cancellationToken);
        return customer;
    }
}
