using Mediator;
using Microsoft.Extensions.Options;
using PayNexa.Common.Caching;
using PayNexa.Customers.Application.Caching;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Options;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.Customers.Domain.Common.Errors;
using PayNexa.SharedKernel.Results;

namespace PayNexa.Customers.Application.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler(ICustomerReadStore readStore, ICacheService cache, IOptions<CustomerOptions> options)
    : IQueryHandler<GetCustomerByIdQuery, Result<CustomerResponse>>
{
    public async ValueTask<Result<CustomerResponse>> Handle(GetCustomerByIdQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = CustomerCacheKeys.Profile(query.CustomerId);

        if (await cache.GetAsync<CustomerResponse>(cacheKey, cancellationToken) is { } cached)
        {
            return cached;
        }

        var customer = await readStore.GetByIdAsync(query.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Errors.Customer.NotFound(query.CustomerId);
        }

        await cache.SetAsync(cacheKey, customer, options.Value.ProfileCacheTimeToLive, cancellationToken);
        return customer;
    }
}
