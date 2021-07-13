using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddPropertiesForMobile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMobileLoggedIn",
                schema: "SystemManagement",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "MobileLastLogin",
                schema: "SystemManagement",
                table: "User",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileSessionId",
                schema: "SystemManagement",
                table: "User",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileTokenId",
                schema: "SystemManagement",
                table: "User",
                type: "varchar(max)",
                unicode: false,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMobileLoggedIn",
                schema: "SystemManagement",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MobileLastLogin",
                schema: "SystemManagement",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MobileSessionId",
                schema: "SystemManagement",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MobileTokenId",
                schema: "SystemManagement",
                table: "User");
        }
    }
}
