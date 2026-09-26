namespace PayNexa.SqlServer;

internal static class SqlServerErrorMessages
{
    public const string ConcurrencyConflict = "The record was modified by another request.";
    public const string UniqueConstraintViolation = "A record with the same unique value already exists.";
    public const string DomainEventCascadeTooDeep = "Domain event handlers kept raising new domain events; the cascade was stopped to prevent an infinite loop.";
    public const string ConnectionStringRequiredFormat = "Connection string '{0}' is required.";
    public const string NonUtcDateTime = "DateTime values persisted to SQL Server must be UTC; received a value with an unspecified kind.";
    public const string MissingValueObjectConstructorFormat = "Value object {0} needs a constructor taking a single {1} so it can be mapped automatically.";
}
