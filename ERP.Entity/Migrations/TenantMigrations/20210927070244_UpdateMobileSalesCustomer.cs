using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class UpdateMobileSalesCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Mark",
                schema: "MobileSales",
                table: "MobileCustomer",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileCustomer",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileCustomer",
                type: "datetime",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mark",
                schema: "MobileSales",
                table: "MobileCustomer");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                schema: "MobileSales",
                table: "MobileCustomer");

            migrationBuilder.DropColumn(
                name: "RejectedDate",
                schema: "MobileSales",
                table: "MobileCustomer");
        }
    }
}
