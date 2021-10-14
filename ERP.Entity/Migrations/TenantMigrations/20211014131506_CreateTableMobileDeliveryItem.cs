using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableMobileDeliveryItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MobileDeliveryItemHeader",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DlvPlanCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
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
                    table.PrimaryKey("PK_MobileDeliveryItemHeader", x => x.Code);
                    table.ForeignKey(
                        name: "FK_MobileDeliveryItemHeader_DeliveryPlanHeader_DlvPlanCode",
                        column: x => x.DlvPlanCode,
                        principalSchema: "Sales",
                        principalTable: "DeliveryPlanHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "MobileDeliveryItemDetail",
                schema: "MobileWarehouse",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileDeliveryItemDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MobileDeliveryItemDetail_Item_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "Item",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileDeliveryItemDetail_MobileDeliveryItemHeader_Code",
                        column: x => x.Code,
                        principalSchema: "MobileWarehouse",
                        principalTable: "MobileDeliveryItemHeader",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_MobileDeliveryItemDetail_UoM_UomId",
                        column: x => x.UomId,
                        principalSchema: "Inventory",
                        principalTable: "UoM",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MobileDeliveryItemDetail_UoMConversion_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "Inventory",
                        principalTable: "UoMConversion",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MobileReceiveItemDetail_Code",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileDeliveryItemDetail_Code",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_MobileDeliveryItemDetail_ItemId",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileDeliveryItemDetail_UnitId",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileDeliveryItemDetail_UomId",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemDetail",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_MobileDeliveryItemHeader_DlvPlanCode",
                schema: "MobileWarehouse",
                table: "MobileDeliveryItemHeader",
                column: "DlvPlanCode");

            migrationBuilder.AddForeignKey(
                name: "FK_MobileReceiveItemDetail_MobileReceiveItemHeader_Code",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail",
                column: "Code",
                principalSchema: "MobileWarehouse",
                principalTable: "MobileReceiveItemHeader",
                principalColumn: "Code");

            // Create view MobileWarehouse.vwMobileReceiveItemHeader
            var sql = @"CREATE VIEW [MobileWarehouse].[vwMobileReceiveItemHeader]
AS
    SELECT mr_h.*,
        s.Initial AS SupInitial,
        e.Initial AS ReceiveInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE mr_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status]
    FROM MobileWarehouse.MobileReceiveItemHeader mr_h
    LEFT JOIN General.Supplier s
        ON s.Code = mr_h.SupCode
    LEFT JOIN General.Employee e
        ON e.Id = mr_h.ReceiveBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mr_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mr_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mr_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mr_h.RejectedBy";
            migrationBuilder.Sql(sql);

            // Create view MobileWarehouse.vwMobileReceiveItemDetail
            sql = @"CREATE VIEW [MobileWarehouse].[vwMobileReceiveItemDetail]
AS
	SELECT rcv_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM MobileWarehouse.MobileReceiveItemDetail rcv_d
	LEFT JOIN Inventory.Item i
		ON i.Id = rcv_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = rcv_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = rcv_d.UnitId";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MobileReceiveItemDetail_MobileReceiveItemHeader_Code",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail");

            migrationBuilder.DropTable(
                name: "MobileDeliveryItemDetail",
                schema: "MobileWarehouse");

            migrationBuilder.DropTable(
                name: "MobileDeliveryItemHeader",
                schema: "MobileWarehouse");

            migrationBuilder.DropIndex(
                name: "IX_MobileReceiveItemDetail_Code",
                schema: "MobileWarehouse",
                table: "MobileReceiveItemDetail");

            // Drop view MobileWarehouse.vwMobileReceiveItemHeader
            var sql = @"DROP VIEW [MobileWarehouse].[vwMobileReceiveItemHeader]";
            migrationBuilder.Sql(sql);

            // Drop view MobileWarehouse.vwMobileReceiveItemDetail
            sql = @"DROP VIEW [MobileWarehouse].[vwMobileReceiveItemDetail]";
            migrationBuilder.Sql(sql);
        }
    }
}
