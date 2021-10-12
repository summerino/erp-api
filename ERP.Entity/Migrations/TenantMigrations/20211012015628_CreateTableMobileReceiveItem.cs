using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableMobileReceiveItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "MobileWarehouse");

            migrationBuilder.CreateTable(
                name: "MobileReceiveItemDetail",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransDetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileReceiveItemDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemDetail_Warehouse_WarehouseCode",
                        column: x => x.WarehouseCode,
                        principalSchema: "Inventory",
                        principalTable: "Warehouse",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileReceiveItemHeader",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    RcvCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    SupCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ReceiveBy = table.Column<long>(type: "bigint", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileReceiveItemHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemHeader_Employee_ReceiveBy",
                        column: x => x.ReceiveBy,
                        principalSchema: "General",
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemHeader_PurchaseReceiveHeader_RcvCode",
                        column: x => x.RcvCode,
                        principalSchema: "Purchasing",
                        principalTable: "PurchaseReceiveHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileReceiveItemHeader_Supplier_SupCode",
                        column: x => x.SupCode,
                        principalSchema: "General",
                        principalTable: "Supplier",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemDetail_ItemId",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemDetail_UnitId",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemDetail_UomId",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemDetail_WarehouseCode",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "WarehouseCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemHeader_RcvCode",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemHeader",
                column: "RcvCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemHeader_ReceiveBy",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemHeader",
                column: "ReceiveBy");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemHeader_SupCode",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemHeader",
                column: "SupCode");

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemHeader_TransCode",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemHeader",
                column: "TransCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobileReceiveItemDetail",
                schema: "MobileWarehouse");

            migrationBuilder.DropTable(
                name: "MobileReceiveItemHeader",
                schema: "MobileWarehouse");
        }
    }
}
