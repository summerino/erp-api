using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableWarehouseQuantity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Purchasing",
                table: "DebitMemo",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "WarehouseQuantity",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    QtyOnHand = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    QtyOnOrder = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    QtyOnIndent = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    QtyReorderPoint = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    QtyOnTransfer = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseQuantity", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarehouseQuantity",
                schema: "Inventory");

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Purchasing",
                table: "DebitMemo",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3);
        }
    }
}
