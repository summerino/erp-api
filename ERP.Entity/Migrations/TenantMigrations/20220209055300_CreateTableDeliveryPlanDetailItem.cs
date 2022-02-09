using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableDeliveryPlanDetailItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryPlanDetailItem",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DlvPlanDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransDetailId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(19,6)", precision: 19, scale: 6, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPlanDetailItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetailItem_DeliveryPlanDetail_DlvPlanDetailId",
                        column: x => x.DlvPlanDetailId,
                        principalSchema: "Sales",
                        principalTable: "DeliveryPlanDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetailItem_DeliveryPlanHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "DeliveryPlanHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetailItem_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetailItem_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryPlanDetailItem_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_Code",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_DlvPlanDetailId",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "DlvPlanDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_ItemId",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_TransDetailId",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "TransDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_UnitId",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetailItem_UomId",
                schema: "Sales",
                table: "DeliveryPlanDetailItem",
                column: "UomId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryPlanDetailItem",
                schema: "Sales");
        }
    }
}
