using FluentValidation;
using PayNexa.Common.Querying;
using PayNexa.Customers.Domain.CustomerAggregate.Enums;

namespace PayNexa.Customers.Application.Queries.ListCustomers;

public sealed class ListCustomersQueryValidator : AbstractValidator<ListCustomersQuery>
{
    public ListCustomersQueryValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        this.RuleForPage(query => query.Page);
        RuleFor(query => query.Search).ValidSearch();
        RuleFor(query => query.Status).ValidEnumFilter<ListCustomersQuery, CustomerStatus>(CustomerQueryParameterNames.Status);
        RuleFor(query => query.KycStatus).ValidEnumFilter<ListCustomersQuery, KycStatus>(CustomerQueryParameterNames.KycStatus);
        RuleFor(query => query.SortBy).ValidSortField(CustomerSorting.Fields);
        RuleFor(query => query.SortOrder).ValidSortOrder(CustomerSorting.Fields);
    }
}
