using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class RemovLOD : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LOD",
                schema: "Accounting",
                table: "COA");

            // Refresh view Accounting.vwCOA
            var sql = @"execute sp_refreshview 'Accounting.vwCOA'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "LOD",
                schema: "Accounting",
                table: "COA",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);
        }
    }
}
