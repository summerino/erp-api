using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTablePromoDetailMultipleItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromoDetailMultipleItem",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromoDetailId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoDetailMultipleItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoDetailMultipleItem_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PromoDetailMultipleItem_PromoDetail_PromoDetailId",
                        column: x => x.PromoDetailId,
                        principalSchema: "Sales",
                        principalTable: "PromoDetail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromoDetail_ItemId",
                schema: "Sales",
                table: "PromoDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoDetailMultipleItem_ItemId",
                schema: "Sales",
                table: "PromoDetailMultipleItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoDetailMultipleItem_PromoDetailId",
                schema: "Sales",
                table: "PromoDetailMultipleItem",
                column: "PromoDetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromoDetailMultipleItem",
                schema: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_PromoDetail_ItemId",
                schema: "Sales",
                table: "PromoDetail");
        }
    }
}
