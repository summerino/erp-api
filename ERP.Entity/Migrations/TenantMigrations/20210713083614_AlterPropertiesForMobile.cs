using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterPropertiesForMobile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MobileIPAddress",
                schema: "SystemManagement",
                table: "User",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileIPAddress",
                schema: "SystemManagement",
                table: "User");
        }
    }
}
