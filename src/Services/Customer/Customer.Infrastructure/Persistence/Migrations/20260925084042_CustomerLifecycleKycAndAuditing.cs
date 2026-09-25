using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayNexa.Customers.Infrastructure.Persistence.Migrations
{
    public partial class CustomerLifecycleKycAndAuditing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressCity",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressCountryCode",
                schema: "customer",
                table: "Customers",
                type: "nchar(2)",
                fixedLength: true,
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressPostalCode",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressState",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<string>(
                name: "KycRejectionReason",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KycStatus",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "customer",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Status_KycStatus",
                schema: "customer",
                table: "Customers",
                columns: new[] { "Status", "KycStatus" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_Status_KycStatus",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressCity",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressCountryCode",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressLine1",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressPostalCode",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "AddressState",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "KycRejectionReason",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "KycStatus",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "StatusReason",
                schema: "customer",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "customer",
                table: "Customers");
        }
    }
}
