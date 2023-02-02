using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    /// <inheritdoc />
    public partial class AddTableItemCogsHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemCogsHistory",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    BaseUnit = table.Column<int>(type: "int", nullable: false),
                    TotalBaseQty = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalBaseNettPrice = table.Column<decimal>(type: "decimal(22,9)", precision: 22, scale: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCogsHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemCogsHistory_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItemCogsHistory_UoMConversion_BaseUnit",
                        column: x => x.BaseUnit,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItemCogsHistory_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemCogsHistory_BaseUnit",
                schema: "Inventory",
                table: "ItemCogsHistory",
                column: "BaseUnit");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCogsHistory_ItemId",
                schema: "Inventory",
                table: "ItemCogsHistory",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCogsHistory_UomId",
                schema: "Inventory",
                table: "ItemCogsHistory",
                column: "UomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemCogsHistory",
                schema: "Inventory");
        }
    }
}
