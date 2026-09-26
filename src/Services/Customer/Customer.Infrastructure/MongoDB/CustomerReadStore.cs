using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using PayNexa.Common.Querying;
using PayNexa.Customers.Application.Interfaces;
using PayNexa.Customers.Application.Queries.ListCustomers;
using PayNexa.Customers.Contracts.Responses;
using PayNexa.MongoDb.Querying;
using SortDirection = PayNexa.Common.Querying.SortDirection;

namespace PayNexa.Customers.Infrastructure.MongoDB;

internal sealed class CustomerReadStore(IMongoCollection<CustomerReadModel> customers) : ICustomerReadStore
{
    private static readonly FilterDefinitionBuilder<CustomerReadModel> Filter = Builders<CustomerReadModel>.Filter;
    private static readonly SortDefinitionBuilder<CustomerReadModel> Sort = Builders<CustomerReadModel>.Sort;

    public async Task<CustomerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        (await customers.Find(model => model.Id == id).FirstOrDefaultAsync(cancellationToken))?.ToResponse();

    public async Task<CustomerResponse?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        (await customers.Find(model => model.Email == normalizedEmail).FirstOrDefaultAsync(cancellationToken))?.ToResponse();

    public Task<PagedResult<CustomerResponse>> ListAsync(CustomerListCriteria criteria, CancellationToken cancellationToken) =>
        customers.ToPagedResultAsync(
            BuildFilter(criteria),
            BuildSort(criteria.Sort),
            criteria.Page,
            model => model.ToResponse(),
            cancellationToken);

    private static FilterDefinition<CustomerReadModel> BuildFilter(CustomerListCriteria criteria)
    {
        var filters = new List<FilterDefinition<CustomerReadModel>>();

        if (criteria.Search is not null)
        {
            var pattern = new BsonRegularExpression(Regex.Escape(criteria.Search), "i");
            filters.Add(Filter.Or(
                Filter.Regex(model => model.FirstName, pattern),
                Filter.Regex(model => model.LastName, pattern),
                Filter.Regex(model => model.Email, pattern)));
        }

        if (criteria.Status is { } status)
        {
            filters.Add(Filter.Eq(model => model.Status, status.ToString()));
        }

        if (criteria.KycStatus is { } kycStatus)
        {
            filters.Add(Filter.Eq(model => model.KycStatus, kycStatus.ToString()));
        }

        return filters.Count == 0 ? Filter.Empty : Filter.And(filters);
    }

    private static SortDefinition<CustomerReadModel> BuildSort(SortRequest<CustomerSortField> sort)
    {
        var ascending = sort.Direction == SortDirection.Ascending;

        var primary = sort.Field switch
        {
            CustomerSortField.UpdatedAt => ascending ? Sort.Ascending(model => model.UpdatedAtUtc) : Sort.Descending(model => model.UpdatedAtUtc),
            CustomerSortField.FirstName => ascending ? Sort.Ascending(model => model.FirstName) : Sort.Descending(model => model.FirstName),
            CustomerSortField.LastName => ascending ? Sort.Ascending(model => model.LastName) : Sort.Descending(model => model.LastName),
            CustomerSortField.Email => ascending ? Sort.Ascending(model => model.Email) : Sort.Descending(model => model.Email),
            _ => ascending ? Sort.Ascending(model => model.CreatedAtUtc) : Sort.Descending(model => model.CreatedAtUtc),
        };

        return Sort.Combine(primary, Sort.Ascending(model => model.Id));
    }
}
