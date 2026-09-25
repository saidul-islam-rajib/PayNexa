using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using PayNexa.Common.Results;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Queries.ListCustomers;
using PayNexa.Customers.Contracts.Responses;
using SortDirection = PayNexa.Common.Querying.SortDirection;

namespace PayNexa.Customers.Infrastructure.MongoDB;

internal sealed class CustomerReadStore(IMongoCollection<CustomerReadModel> customers) : ICustomerReadStore
{
    public async Task<CustomerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customers
            .Find(model => model.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return customer?.ToResponse();
    }

    public async Task<PagedResult<CustomerResponse>> ListAsync(CustomerListCriteria criteria, CancellationToken cancellationToken)
    {
        var filter = BuildFilter(criteria.Search);

        var totalCount = await customers.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var page = await customers
            .Find(filter)
            .Sort(BuildSort(criteria.SortBy, criteria.SortDirection))
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Limit(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerResponse>(
            page.Select(model => model.ToResponse()).ToList(),
            criteria.Page,
            criteria.PageSize,
            totalCount);
    }

    private static FilterDefinition<CustomerReadModel> BuildFilter(string? search)
    {
        var filter = Builders<CustomerReadModel>.Filter;

        if (search is null)
        {
            return filter.Empty;
        }

        var pattern = new BsonRegularExpression(Regex.Escape(search), "i");

        return filter.Or(
            filter.Regex(model => model.FirstName, pattern),
            filter.Regex(model => model.LastName, pattern),
            filter.Regex(model => model.Email, pattern));
    }

    private static SortDefinition<CustomerReadModel> BuildSort(CustomerSortField field, SortDirection direction)
    {
        var sort = Builders<CustomerReadModel>.Sort;
        var primary = (field, direction) switch
        {
            (CustomerSortField.FirstName, SortDirection.Ascending) => sort.Ascending(model => model.FirstName),
            (CustomerSortField.FirstName, _) => sort.Descending(model => model.FirstName),
            (CustomerSortField.LastName, SortDirection.Ascending) => sort.Ascending(model => model.LastName),
            (CustomerSortField.LastName, _) => sort.Descending(model => model.LastName),
            (CustomerSortField.Email, SortDirection.Ascending) => sort.Ascending(model => model.Email),
            (CustomerSortField.Email, _) => sort.Descending(model => model.Email),
            (_, SortDirection.Ascending) => sort.Ascending(model => model.CreatedAtUtc),
            _ => sort.Descending(model => model.CreatedAtUtc),
        };

        return sort.Combine(primary, sort.Ascending(model => model.Id));
    }
}
