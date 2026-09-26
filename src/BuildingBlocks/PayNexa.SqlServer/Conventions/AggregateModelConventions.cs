using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PayNexa.SharedKernel.Domain;

namespace PayNexa.SqlServer.Conventions;

public static class AggregateModelConventions
{
    public const int ActorMaxLength = 100;
    private const string AuditTimestampColumnType = "datetime2(3)";

    public static ModelBuilder ApplySingleValueObjectConversions(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToArray())
        {
            var properties = entityType.ClrType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => SingleValueTypeOf(property.PropertyType) is not null);

            foreach (var property in properties)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion(CreateConverter(property.PropertyType, SingleValueTypeOf(property.PropertyType)!));
            }
        }

        return modelBuilder;
    }

    public static ModelBuilder ApplyAuditingConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(type => typeof(IAuditable).IsAssignableFrom(type.ClrType)).ToArray())
        {
            var entity = modelBuilder.Entity(entityType.ClrType);

            entity.Property(nameof(IAuditable.CreatedAtUtc)).HasColumnType(AuditTimestampColumnType);
            entity.Property(nameof(IAuditable.UpdatedAtUtc)).HasColumnType(AuditTimestampColumnType);
            entity.Property(nameof(IAuditable.CreatedBy)).HasMaxLength(ActorMaxLength).IsRequired();
            entity.Property(nameof(IAuditable.UpdatedBy)).HasMaxLength(ActorMaxLength).IsRequired();

            if (typeof(IVersioned).IsAssignableFrom(entityType.ClrType))
            {
                entity.Property(nameof(IVersioned.Version)).IsConcurrencyToken();
            }

            if (typeof(IHasDomainEvents).IsAssignableFrom(entityType.ClrType))
            {
                entity.Ignore(nameof(IHasDomainEvents.DomainEvents));
            }
        }

        return modelBuilder;
    }

    private static Type? SingleValueTypeOf(Type type)
    {
        for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(SingleValueObject<>))
            {
                return current.GetGenericArguments()[0];
            }
        }

        return null;
    }

    private static ValueConverter CreateConverter(Type valueObjectType, Type valueType)
    {
        var constructor = valueObjectType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, [valueType])
                          ?? throw new InvalidOperationException(string.Format(SqlServerErrorMessages.MissingValueObjectConstructorFormat, valueObjectType.Name, valueType.Name));

        var model = Expression.Parameter(valueObjectType, "model");
        var toProvider = Expression.Lambda(Expression.Property(model, nameof(SingleValueObject<int>.Value)), model);

        var provider = Expression.Parameter(valueType, "provider");
        var fromProvider = Expression.Lambda(Expression.New(constructor, provider), provider);

        var converterType = typeof(ValueConverter<,>).MakeGenericType(valueObjectType, valueType);
        return (ValueConverter)Activator.CreateInstance(converterType, toProvider, fromProvider, null)!;
    }
}
