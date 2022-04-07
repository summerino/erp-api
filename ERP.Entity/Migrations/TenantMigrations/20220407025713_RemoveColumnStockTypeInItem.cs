using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class RemoveColumnStockTypeInItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockType",
                schema: "Inventory",
                table: "Item");

            // Refresh view Inventory.vwItem
            var sql = @"exec sp_refreshview 'Inventory.vwItem'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "StockType",
                schema: "Inventory",
                table: "Item",
                type: "smallint",
                nullable: true);

            // Refresh view Inventory.vwItem
            var sql = @"exec sp_refreshview 'Inventory.vwItem'";
            migrationBuilder.Sql(sql);
        }
    }
}
