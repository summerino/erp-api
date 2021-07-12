using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterTableStockMutationAndOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdjustmentDetailWithDifferentUnit",
                schema: "Inventory");

            migrationBuilder.AlterColumn<string>(
                name: "Src",
                schema: "Inventory",
                table: "StockMutation",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldUnicode: false,
                oldMaxLength: 5);

            migrationBuilder.CreateTable(
                name: "AdjustmentDetailDiffUnit",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentDetailId = table.Column<long>(type: "bigint", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    QtyAdjust = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjustmentDetailDiffUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdjustmentDetailDiffUnit_AdjustmentDetail_AdjustmentDetailId",
                        column: x => x.AdjustmentDetailId,
                        principalSchema: "Inventory",
                        principalTable: "AdjustmentDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdjustmentDetailDiffUnit_AdjustmentDetailId",
                schema: "Inventory",
                table: "AdjustmentDetailDiffUnit",
                column: "AdjustmentDetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdjustmentDetailDiffUnit",
                schema: "Inventory");

            migrationBuilder.AlterColumn<string>(
                name: "Src",
                schema: "Inventory",
                table: "StockMutation",
                type: "varchar(5)",
                unicode: false,
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.CreateTable(
                name: "AdjustmentDetailWithDifferentUnit",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdjustmentDetailId = table.Column<long>(type: "bigint", nullable: false),
                    QtyAdjust = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjustmentDetailWithDifferentUnit", x => x.Id);
                });
        }
    }
}
