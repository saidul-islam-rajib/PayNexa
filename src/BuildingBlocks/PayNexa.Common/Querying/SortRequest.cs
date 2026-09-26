namespace PayNexa.Common.Querying;

public sealed record SortRequest<TField>(TField Field, SortDirection Direction)
    where TField : struct, Enum;
