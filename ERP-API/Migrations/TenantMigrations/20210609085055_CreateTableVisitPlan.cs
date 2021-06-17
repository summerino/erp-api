using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
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

            // Create view Sales.vwVisitPlanHeader
            sql = @"CREATE VIEW [Sales].[vwVisitPlanHeader]
AS
    SELECT vph.*,
		sg.Initial AS GroupInitial,
		sg.[Name] AS GroupName,
		e.FirstName + ' ' + e.LastName AS SupervisorName,
		u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE vph.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
	FROM Sales.VisitPlanHeader vph
	LEFT JOIN Sales.SalesmanGroup sg
		ON vph.GroupId = sg.Id
	LEFT JOIN General.Employee e
		ON sg.SupervisorId = e.Id
	LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = vph.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = vph.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = vph.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitPlanDetail
            sql = @"CREATE VIEW [Sales].[vwVisitPlanDetail]
AS
    SELECT vpd.Id AS VisitPlanDetailId, 
		vpd.Code, e.*, 
		ss.AreaId1,
		ss.AreaId2,
		ss.AreaId3,
		ss.AreaId4,
		ss.AreaId5,
		ss.StartDate,
		ss.EndDate,
		ss.Recurrence,
		ss.VisitDay,
		LTRIM(RTRIM(e.FirstName)) + ' ' + LTRIM(RTRIM(e.LastName)) AS FullName,
		sg.Initial AS GroupInitial,
		sg.[Name] AS GroupNameInitial,
		CASE WHEN e.Sex = 1 
			THEN 'Pria' 
			ELSE 'Wanita' 
			END AS GenderInitial
	FROM Sales.VisitPlanDetail vpd
	INNER JOIN General.Employee e 
		ON vpd.SalesmanId = e.Id
	LEFT JOIN Sales.SalesmanSchedule ss
		ON vpd.SalesmanId = ss.SalesmanId
	LEFT JOIN Sales.SalesmanGroup sg
		ON e.SalesGroupId = sg.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitPlanDetailCustomer
            sql = @"CREATE VIEW [Sales].[vwVisitPlanDetailCustomer]
AS
    SELECT vpdc.VisitPlanDetailId, 
		   c.Code,
		   c.Initial,
		   c.[Name],
		   a_1.[Name] AS AreaName1,
		   a_2.[Name] AS AreaName2,
		   a_3.[Name] AS AreaName3,
		   a_4.[Name] AS AreaName4,
		   a_5.[Name] AS AreaName5,
		   c.Email,
		   c.Website,
		   c.TypeId
	FROM Sales.VisitPlanDetailCustomer vpdc
	INNER JOIN General.Customer c
		ON vpdc.CustCode = c.Code
	LEFT JOIN Sales.Area a_1
		ON c.AreaId1 = a_1.Id
	LEFT JOIN Sales.Area a_2
		ON c.AreaId2 = a_2.Id
	LEFT JOIN Sales.Area a_3
		ON c.AreaId3 = a_3.Id
	LEFT JOIN Sales.Area a_4
		ON c.AreaId4 = a_4.Id
	LEFT JOIN Sales.Area a_5
		ON c.AreaId5 = a_5.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesmanSchedule
            sql = @"CREATE VIEW [Sales].[vwSalesmanSchedule]
AS
    SELECT ss.Id AS SalesmanScheduleId, 
		e.*,
		a_1.[Name] AS AreaName1,
		a_2.[Name] AS AreaName2,
		a_3.[Name] AS AreaName3,
		a_4.[Name] AS AreaName4,
		a_5.[Name] AS AreaName5,
		ss.StartDate,
		ss.EndDate,
		ss.Recurrence,
		ss.VisitDay,
		LTRIM(RTRIM(e.FirstName)) + ' ' + LTRIM(RTRIM(e.LastName)) AS FullName
	FROM Sales.SalesmanSchedule ss
	LEFT JOIN General.Employee e 
		ON ss.SalesmanId = e.Id
	LEFT JOIN Sales.Area a_1
		ON ss.AreaId1 = a_1.Id
	LEFT JOIN Sales.Area a_2
		ON ss.AreaId2 = a_2.Id
	LEFT JOIN Sales.Area a_3
		ON ss.AreaId3 = a_3.Id
	LEFT JOIN Sales.Area a_4
		ON ss.AreaId4 = a_4.Id
	LEFT JOIN Sales.Area a_5
		ON ss.AreaId5 = a_5.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesmanScheduleCustomer
            sql = @"CREATE VIEW [Sales].[vwSalesmanScheduleCustomer]
AS
    SELECT ss.Id AS SalesmanScheduleId, 
		c.*,
		a_1.[Name] AS AreaName1,
		a_2.[Name] AS AreaName2,
		a_3.[Name] AS AreaName3,
		a_4.[Name] AS AreaName4,
		a_5.[Name] AS AreaName5
	FROM Sales.SalesmanScheduleCustomer ssc 
	LEFT JOIN Sales.SalesmanSchedule ss
		ON ssc.SalesmanScheduleId = ss.Id
	LEFT JOIN General.Customer c
		ON ssc.CustCode = c.Code
	LEFT JOIN Sales.Area a_1
		ON c.AreaId1 = a_1.Id
	LEFT JOIN Sales.Area a_2
		ON c.AreaId2 = a_2.Id
	LEFT JOIN Sales.Area a_3
		ON c.AreaId3 = a_3.Id
	LEFT JOIN Sales.Area a_4
		ON c.AreaId4 = a_4.Id
	LEFT JOIN Sales.Area a_5
		ON c.AreaId5 = a_5.Id";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwVisitOrder
            sql = @"SELECT vo.*,
		voc.CustCode,
		c.[Name] AS CustomerName,
		voi.InvCode,
		CASE vo.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status],
		u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial
	FROM Sales.VisitOrder vo 
	LEFT JOIN Sales.VisitOrderCustomer voc
		ON vo.Code = voc.Code
	LEFT JOIN Sales.VisitOrderInvoice voi
		ON vo.Code = voi.Code
	LEFT JOIN General.Customer c
		ON voc.CustCode = c.Code
	LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = vo.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = vo.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = vo.ApprovedBy";
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

            // Drop view
            var sql = @"DROP VIEW [Sales].[vwVisitPlanHeader]";
            migrationBuilder.Sql(sql);

            sql = @"DROP VIEW [Sales].[vwVisitPlanDetail]";
            migrationBuilder.Sql(sql);

            sql = @"DROP VIEW [Sales].[vwVisitPlanDetailCustomer]";
            migrationBuilder.Sql(sql);

            sql = @"DROP VIEW [Sales].[vwSalesmanSchedule]";
            migrationBuilder.Sql(sql);

            sql = @"DROP VIEW [Sales].[vwSalesmanScheduleCustomer]";
            migrationBuilder.Sql(sql);

            sql = @"DROP VIEW [Sales].[vwVisitOrder]";
            migrationBuilder.Sql(sql);
        }
    }
}
