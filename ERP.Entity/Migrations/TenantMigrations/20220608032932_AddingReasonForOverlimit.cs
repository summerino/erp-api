using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingReasonForOverlimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OverlimitApprovedReason",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "varchar(256)",
                unicode: false,
                maxLength: 256,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 109);

            // Refresh view Sales.vwSalesOrderHeader
            var sql = @"EXEC sp_refreshview 'Sales.vwSalesOrderHeader'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverlimitApprovedReason",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
