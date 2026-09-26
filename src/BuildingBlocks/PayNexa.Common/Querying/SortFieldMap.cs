namespace PayNexa.Common.Querying;

public sealed class SortFieldMap<TField>
    where TField : struct, Enum
{
    public const string Ascending = "asc";
    public const string Descending = "desc";

    private static readonly Dictionary<string, SortDirection> Directions = new(StringComparer.OrdinalIgnoreCase)
    {
        [Ascending] = SortDirection.Ascending,
        [Descending] = SortDirection.Descending,
    };

    private readonly Dictionary<string, TField> _fields;

    public SortFieldMap(IReadOnlyDictionary<string, TField> fields, SortRequest<TField> defaultSort)
    {
        _fields = new Dictionary<string, TField>(fields, StringComparer.OrdinalIgnoreCase);
        Default = defaultSort;
    }

    public SortRequest<TField> Default { get; }

    public IReadOnlyCollection<string> FieldNames => _fields.Keys;

    public static IReadOnlyCollection<string> DirectionNames => Directions.Keys;

    public bool IsValidField(string? sortBy) => sortBy is null || _fields.ContainsKey(sortBy);

    public static bool IsValidDirection(string? sortOrder) => sortOrder is null || Directions.ContainsKey(sortOrder);

    public SortRequest<TField> Resolve(string? sortBy, string? sortOrder) => new(
        sortBy is not null && _fields.TryGetValue(sortBy, out var field) ? field : Default.Field,
        sortOrder is not null && Directions.TryGetValue(sortOrder, out var direction) ? direction : Default.Direction);
}
