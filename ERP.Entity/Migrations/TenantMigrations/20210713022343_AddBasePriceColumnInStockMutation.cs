using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddBasePriceColumnInStockMutation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseNettPrice",
                schema: "Inventory",
                table: "StockMutation",
                type: "decimal(19,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NettPrice",
                schema: "Inventory",
                table: "StockMutation",
                type: "decimal(19,6)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseNettPrice",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropColumn(
                name: "NettPrice",
                schema: "Inventory",
                table: "StockMutation");
        }
    }
}
