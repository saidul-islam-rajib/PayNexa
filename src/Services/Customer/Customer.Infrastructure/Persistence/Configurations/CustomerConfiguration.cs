using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayNexa.Customers.Domain.CustomerAggregate;
using PayNexa.Customers.Domain.CustomerAggregate.ValueObjects;

namespace PayNexa.Customers.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    private const string TableName = "Customers";
    private const int EnumMaxLength = 20;

    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        ConfigureCustomersTable(builder);
        ConfigureName(builder);
        ConfigureContactDetails(builder);
        ConfigureAddress(builder);
        ConfigureLifecycle(builder);
    }

    private static void ConfigureCustomersTable(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Id).ValueGeneratedNever();
        builder.Property(customer => customer.DateOfBirth).HasColumnType("date");
        builder.HasIndex(customer => customer.CreatedAtUtc);
    }

    private static void ConfigureName(EntityTypeBuilder<Customer> builder) =>
        builder.ComplexProperty(customer => customer.Name, name =>
        {
            name.Property(value => value.FirstName)
                .HasColumnName(nameof(PersonName.FirstName))
                .HasMaxLength(PersonName.MaxLength)
                .IsRequired();

            name.Property(value => value.LastName)
                .HasColumnName(nameof(PersonName.LastName))
                .HasMaxLength(PersonName.MaxLength)
                .IsRequired();
        });

    private static void ConfigureContactDetails(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(customer => customer.Email).HasMaxLength(Email.MaxLength).IsRequired();
        builder.HasIndex(customer => customer.Email).IsUnique();

        builder.Property(customer => customer.PhoneNumber).HasMaxLength(PhoneNumber.MaxLength).IsRequired();
    }

    private static void ConfigureAddress(EntityTypeBuilder<Customer> builder) =>
        builder.OwnsOne(customer => customer.Address, address =>
        {
            address.Property(value => value.Line1).HasColumnName("AddressLine1").HasMaxLength(Address.LineMaxLength);
            address.Property(value => value.Line2).HasColumnName("AddressLine2").HasMaxLength(Address.LineMaxLength);
            address.Property(value => value.City).HasColumnName("AddressCity").HasMaxLength(Address.CityMaxLength);
            address.Property(value => value.State).HasColumnName("AddressState").HasMaxLength(Address.StateMaxLength);
            address.Property(value => value.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(Address.PostalCodeMaxLength);
            address.Property(value => value.CountryCode).HasColumnName("AddressCountryCode").HasMaxLength(Address.CountryCodeLength).IsFixedLength();
        });

    private static void ConfigureLifecycle(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(customer => customer.Status).HasConversion<string>().HasMaxLength(EnumMaxLength).IsRequired();
        builder.Property(customer => customer.StatusReason).HasMaxLength(StatusReason.MaxLength);
        builder.Property(customer => customer.KycStatus).HasConversion<string>().HasMaxLength(EnumMaxLength).IsRequired();
        builder.Property(customer => customer.KycRejectionReason).HasMaxLength(StatusReason.MaxLength);
        builder.HasIndex(customer => new { customer.Status, customer.KycStatus });
    }
}
