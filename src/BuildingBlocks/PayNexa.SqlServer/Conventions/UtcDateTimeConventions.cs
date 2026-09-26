using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PayNexa.SqlServer.Conventions;

public static class UtcDateTimeConventions
{
    public static ModelConfigurationBuilder UseUtcDateTimes(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
        return configurationBuilder;
    }

    private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
        value => ToUtc(value),
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private sealed class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
        value => value.HasValue ? ToUtc(value.Value) : value,
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => throw new InvalidOperationException(SqlServerErrorMessages.NonUtcDateTime),
    };
}
