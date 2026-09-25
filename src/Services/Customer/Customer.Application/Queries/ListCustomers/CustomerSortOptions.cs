using System.Diagnostics.CodeAnalysis;
using PayNexa.Common.Querying;

namespace PayNexa.Customers.Application.Queries.ListCustomers;

public static class CustomerSortOptions
{
    public const string DefaultSortBy = "created-at";
    public const string DefaultSortOrder = "desc";

    private static readonly Dictionary<string, CustomerSortField> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["created-at"] = CustomerSortField.CreatedAt,
        ["first-name"] = CustomerSortField.FirstName,
        ["last-name"] = CustomerSortField.LastName,
        ["email"] = CustomerSortField.Email,
    };

    private static readonly Dictionary<string, SortDirection> Directions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["asc"] = SortDirection.Ascending,
        ["desc"] = SortDirection.Descending,
    };

    public static IReadOnlyCollection<string> SortByValues => Fields.Keys;

    public static IReadOnlyCollection<string> SortOrderValues => Directions.Keys;

    public static bool TryParseField(string? value, [NotNullWhen(true)] out CustomerSortField? field)
    {
        field = Fields.TryGetValue(value ?? DefaultSortBy, out var parsed) ? parsed : null;
        return field is not null;
    }

    public static bool TryParseDirection(string? value, [NotNullWhen(true)] out SortDirection? direction)
    {
        direction = Directions.TryGetValue(value ?? DefaultSortOrder, out var parsed) ? parsed : null;
        return direction is not null;
    }
}
