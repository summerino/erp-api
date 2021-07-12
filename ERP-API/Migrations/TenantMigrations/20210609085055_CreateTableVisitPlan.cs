using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateTableVisitPlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesmanSchedule",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false),
                    AreaId1 = table.Column<int>(type: "int", nullable: true),
                    AreaId2 = table.Column<int>(type: "int", nullable: true),
                    AreaId3 = table.Column<int>(type: "int", nullable: true),
                    AreaId4 = table.Column<int>(type: "int", nullable: true),
                    AreaId5 = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Recurrence = table.Column<byte>(type: "tinyint", nullable: false),
                    VisitDay = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesmanSchedule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesmanScheduleCustomer",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesmanScheduleId = table.Column<long>(type: "bigint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesmanScheduleCustomer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitPlanDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    SalesmanId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitPlanDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitPlanDetailCustomer",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitPlanDetailId = table.Column<long>(type: "bigint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitPlanDetailCustomer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitPlanHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitPlanHeader", x => x.Code);
                });

            // Alter view Sales.vwSalesDeliveryDetail
            var sql = @"ALTER VIEW [Sales].[vwSalesDeliveryDetail]
AS
	SELECT do_d.*,
		CASE WHEN do_h.SrcTrans = 1 THEN ISNULL(so_d.Qty, 0)
			 ELSE ISNULL(rtn_d.Qty, 0) END AS OrderQty,
		CASE WHEN do_h.SrcTrans = 1 THEN ISNULL(so_d.Qty, 0) - ISNULL(so_d.QtyDlv, 0)
			ELSE ISNULL(rtn_d.Qty, 0) - ISNULL(rtn_d.QtyDlv, 0) END AS OutstandingQty,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Sales.SalesDeliveryDetail do_d
	LEFT JOIN Sales.SalesDeliveryHeader do_h
		ON do_h.Code = do_d.Code
	LEFT JOIN Sales.SalesOrderDetail so_d
		ON so_d.Id = do_d.SODetailId
		AND do_h.SrcTrans = 1
	LEFT JOIN Sales.SalesReturnDetail rtn_d
		ON rtn_d.Id = do_d.SODetailId
		AND do_h.SrcTrans = 2
	LEFT JOIN Inventory.Item i
		ON i.Id = do_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = do_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = do_d.UnitId";
            migrationBuilder.Sql(sql);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesmanSchedule",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesmanScheduleCustomer",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "VisitPlanDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "VisitPlanDetailCustomer",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "VisitPlanHeader",
                schema: "Sales");
        }
    }
}
