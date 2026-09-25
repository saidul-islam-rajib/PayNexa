using FluentValidation;

namespace PayNexa.Customers.Application.Queries.ListCustomers;

public sealed class ListCustomersQueryValidator : AbstractValidator<ListCustomersQuery>
{
    public ListCustomersQueryValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, ListCustomersQuery.MaxPageSize);
        RuleFor(query => query.Search).MaximumLength(ListCustomersQuery.MaxSearchLength);
        RuleFor(query => query.SortBy)
            .Must(value => CustomerSortOptions.TryParseField(value, out _))
            .WithMessage($"'{{PropertyName}}' must be one of: {string.Join(", ", CustomerSortOptions.SortByValues)}.");
        RuleFor(query => query.SortOrder)
            .Must(value => CustomerSortOptions.TryParseDirection(value, out _))
            .WithMessage($"'{{PropertyName}}' must be one of: {string.Join(", ", CustomerSortOptions.SortOrderValues)}.");
    }
}
