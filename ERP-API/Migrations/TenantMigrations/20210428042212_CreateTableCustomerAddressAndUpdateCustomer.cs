using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateTableCustomerAddressAndUpdateCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerAddress",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Address1 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Address2 = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ContactPerson = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Fax = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    IsDefault = table.Column<int>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAddress", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "BillingAddressId",
                schema: "General",
                table: "Customer",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingAddressId",
                schema: "General",
                table: "Customer",
                type: "int",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "Address1",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Address2",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "Fax",
                schema: "General",
                table: "Customer");

            // Alter view General.vwCustomer
            var sql = @"ALTER VIEW [General].[vwCustomer]
AS
	SELECT c.*, 
		ca.Initial AS InitialAddress, 
		ca.Address1, 
		ca.Address2, 
		ca.ContactPerson,
		ca.Phone,
		ca.Fax,
		ISNULL(ca.IsDefault, 0) AS IsDefault,
		t.[Name] AS TypeName,
		u.Initial AS UpdatedInitial
	FROM General.Customer c
	LEFT JOIN General.CustomerType t
		ON t.Id = c.TypeId
	LEFT JOIN General.CustomerAddress ca
		ON c.Code = ca.Code AND ca.IsDefault = 1
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerAddress",
                schema: "General");

            migrationBuilder.DropColumn(
                name: "BillingAddressId",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "ShippingAddressId",
                schema: "General",
                table: "Customer");

            migrationBuilder.AddColumn<string>(
                name: "Address1",
                schema: "General",
                table: "Customer",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address2",
                schema: "General",
                table: "Customer",
                type: "varchar(100)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "General",
                table: "Customer",
                type: "varchar(30)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                schema: "General",
                table: "Customer",
                type: "varchar(15)",
                nullable: true,
                defaultValue: "");

            // Alter view General.vwCustomer
            var sql = @"ALTER VIEW [General].[vwCustomer]
AS
    SELECT c.*,
        t.[Name] AS TypeName,
        u.Initial AS UpdatedInitial
    FROM General.Customer c
    LEFT JOIN General.CustomerType t
        ON t.Id = c.TypeId
    LEFT JOIN SystemManagement.[User] u
        ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);
        }
    }
}
