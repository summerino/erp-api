using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class RemoveNewCodeEntityFromMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewCodes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewCodes",
                columns: table => new
                {
                    Value = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                });
        }
    }
}
