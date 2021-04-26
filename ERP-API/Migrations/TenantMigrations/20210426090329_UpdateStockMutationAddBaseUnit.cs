using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class UpdateStockMutationAddBaseUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseQty",
                schema: "Inventory",
                table: "StockMutation",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BaseUnit",
                schema: "Inventory",
                table: "StockMutation",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseQty",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropColumn(
                name: "BaseUnit",
                schema: "Inventory",
                table: "StockMutation");
        }
    }
}
