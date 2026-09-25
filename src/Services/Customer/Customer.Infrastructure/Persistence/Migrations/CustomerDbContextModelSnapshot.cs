using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PayNexa.Customers.Infrastructure.Persistence;

#nullable disable

namespace PayNexa.Customers.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(CustomerDbContext))]
    partial class CustomerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("customer")
                .HasAnnotation("ProductVersion", "10.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PayNexa.Customers.Domain.CustomerAggregate.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2(3)");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateOnly>("DateOfBirth")
                        .HasColumnType("date");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(254)
                        .HasColumnType("nvarchar(254)");

                    b.Property<string>("KycRejectionReason")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("KycStatus")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("nvarchar(16)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("StatusReason")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("UpdatedAtUtc")
                        .HasColumnType("datetime2(3)");

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<long>("Version")
                        .IsConcurrencyToken()
                        .HasColumnType("bigint");

                    b.ComplexProperty(typeof(Dictionary<string, object>), "Name", "PayNexa.Customers.Domain.CustomerAggregate.Customer.Name#PersonName", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<string>("FirstName")
                                .IsRequired()
                                .HasMaxLength(100)
                                .HasColumnType("nvarchar(100)")
                                .HasColumnName("FirstName");

                            b1.Property<string>("LastName")
                                .IsRequired()
                                .HasMaxLength(100)
                                .HasColumnType("nvarchar(100)")
                                .HasColumnName("LastName");
                        });

                    b.HasKey("Id");

                    b.HasIndex("CreatedAtUtc");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Status", "KycStatus");

                    b.ToTable("Customers", "customer");
                });

            modelBuilder.Entity("PayNexa.SqlServer.Outbox.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Attempts")
                        .HasColumnType("int");

                    b.Property<string>("CorrelationId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("LastError")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<DateTime?>("NextAttemptAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("OccurredAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ProcessedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("TraceParent")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.HasKey("Id");

                    b.HasIndex("NextAttemptAtUtc", "OccurredAtUtc")
                        .HasDatabaseName("IX_OutboxMessages_Pending")
                        .HasFilter("[ProcessedAtUtc] IS NULL");

                    b.ToTable("OutboxMessages", "customer");
                });

            modelBuilder.Entity("PayNexa.Customers.Domain.CustomerAggregate.Customer", b =>
                {
                    b.OwnsOne("PayNexa.Customers.Domain.CustomerAggregate.ValueObjects.Address", "Address", b1 =>
                        {
                            b1.Property<Guid>("CustomerId")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasMaxLength(100)
                                .HasColumnType("nvarchar(100)")
                                .HasColumnName("AddressCity");

                            b1.Property<string>("CountryCode")
                                .IsRequired()
                                .HasMaxLength(2)
                                .HasColumnType("nchar(2)")
                                .HasColumnName("AddressCountryCode")
                                .IsFixedLength();

                            b1.Property<string>("Line1")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("nvarchar(200)")
                                .HasColumnName("AddressLine1");

                            b1.Property<string>("Line2")
                                .HasMaxLength(200)
                                .HasColumnType("nvarchar(200)")
                                .HasColumnName("AddressLine2");

                            b1.Property<string>("PostalCode")
                                .IsRequired()
                                .HasMaxLength(20)
                                .HasColumnType("nvarchar(20)")
                                .HasColumnName("AddressPostalCode");

                            b1.Property<string>("State")
                                .HasMaxLength(100)
                                .HasColumnType("nvarchar(100)")
                                .HasColumnName("AddressState");

                            b1.HasKey("CustomerId");

                            b1.ToTable("Customers", "customer");

                            b1.WithOwner()
                                .HasForeignKey("CustomerId");
                        });

                    b.Navigation("Address");
                });
#pragma warning restore 612, 618
        }
    }
}
