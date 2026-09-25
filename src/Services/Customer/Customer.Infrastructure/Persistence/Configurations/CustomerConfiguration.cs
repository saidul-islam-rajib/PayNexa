using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PayNexa.Customers.Domain.Entities;
using PayNexa.Customers.Domain.ValueObjects;

namespace PayNexa.Customers.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    private const int StatusMaxLength = 20;

    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Id).ValueGeneratedNever();

        builder.Property(customer => customer.FirstName).HasMaxLength(Customer.NameMaxLength).IsRequired();
        builder.Property(customer => customer.LastName).HasMaxLength(Customer.NameMaxLength).IsRequired();

        builder.Property(customer => customer.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasMaxLength(Email.MaxLength)
            .IsRequired();
        builder.HasIndex(customer => customer.Email).IsUnique();

        builder.Property(customer => customer.PhoneNumber)
            .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value))
            .HasMaxLength(PhoneNumber.MaxLength)
            .IsRequired();

        builder.Property(customer => customer.DateOfBirth).HasColumnType("date");

        builder.Property(customer => customer.Status)
            .HasConversion<string>()
            .HasMaxLength(StatusMaxLength)
            .IsRequired();

        builder.Property(customer => customer.CreatedAtUtc).HasColumnType("datetime2(3)");
        builder.Property(customer => customer.UpdatedAtUtc).HasColumnType("datetime2(3)");
        builder.Property(customer => customer.Version).IsConcurrencyToken();

        builder.HasIndex(customer => customer.CreatedAtUtc);
    }
}
