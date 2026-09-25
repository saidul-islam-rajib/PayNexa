using System.Linq.Expressions;
using FluentValidation;
using PayNexa.Common.Validation;

namespace PayNexa.Common.Querying;

public static class QueryValidationExtensions
{
    public const int MaxSearchLength = 100;

    public static AbstractValidator<T> RuleForPage<T>(this AbstractValidator<T> validator, Expression<Func<T, PageRequest>> page)
    {
        var select = page.Compile();

        validator.RuleFor(query => select(query).Page)
            .GreaterThanOrEqualTo(PageRequest.DefaultPage)
            .OverridePropertyName(QueryParameterNames.Page);

        validator.RuleFor(query => select(query).PageSize)
            .InclusiveBetween(1, PageRequest.MaxPageSize)
            .OverridePropertyName(QueryParameterNames.PageSize);

        return validator;
    }

    public static IRuleBuilderOptions<T, string?> ValidSearch<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(MaxSearchLength).OverridePropertyName(QueryParameterNames.Search);

    public static IRuleBuilderOptions<T, string?> ValidEnumFilter<T, TEnum>(this IRuleBuilder<T, string?> rule, string parameterName)
        where TEnum : struct, Enum =>
        rule.Must(value => value is null || Enum.TryParse<TEnum>(value, ignoreCase: true, out _))
            .WithMessage(ValidationMessages.OneOf(Enum.GetNames<TEnum>()))
            .OverridePropertyName(parameterName);

    public static TEnum? ParseEnumFilter<TEnum>(string? value)
        where TEnum : struct, Enum =>
        value is not null && Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : null;

    public static IRuleBuilderOptions<T, string?> ValidSortField<T, TField>(this IRuleBuilder<T, string?> rule, SortFieldMap<TField> map)
        where TField : struct, Enum =>
        rule.Must(map.IsValidField)
            .WithMessage(ValidationMessages.OneOf(map.FieldNames))
            .OverridePropertyName(QueryParameterNames.SortBy);

    public static IRuleBuilderOptions<T, string?> ValidSortOrder<T, TField>(this IRuleBuilder<T, string?> rule, SortFieldMap<TField> map)
        where TField : struct, Enum =>
        rule.Must(SortFieldMap<TField>.IsValidDirection)
            .WithMessage(ValidationMessages.OneOf(SortFieldMap<TField>.DirectionNames))
            .OverridePropertyName(QueryParameterNames.SortOrder);
}
