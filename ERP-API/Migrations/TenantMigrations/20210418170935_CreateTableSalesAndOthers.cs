using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateTableSalesAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Sales");

            migrationBuilder.RenameColumn(
                name: "CoaCogs",
                schema: "dbo",
                table: "trPurchaseOrderDetails",
                newName: "CoaCOGS");

            migrationBuilder.CreateTable(
                name: "SalesDeliveryDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    SODetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDeliveryDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesDeliveryHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SOCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    ShippedBy = table.Column<long>(type: "bigint", nullable: false),
                    ApproveBy = table.Column<long>(type: "bigint", nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalDiscPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesDeliveryHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    DOCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesInvoiceHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    SOCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    IssuedBy = table.Column<long>(type: "bigint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesInvoiceHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Length = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    DimensionMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    WeightMeasurement = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    QtyDlv = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Disc = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CoaInventory = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaCOGS = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaSls = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaSlsDisc = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true),
                    CoaSlsReturn = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    SalesBy = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalDiscPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderHeader", x => x.Code);
                });

            // Create view dbo.vw_customers
            var sql = @"CREATE VIEW [dbo].[vw_customers]
AS
	SELECT c.*,
		t.[Name] AS TypeName
	FROM msCustomers c
	LEFT JOIN msCustomerTypes t
		ON t.Id = c.TypeId";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesOrderHeader
            sql = @"CREATE VIEW [Sales].[vwSalesOrderHeader]
AS
	SELECT so_h.*,
		c.[Name] AS CustName,
		e.Initial AS SalesInitial
	FROM Sales.SalesOrderHeader so_h
	LEFT JOIN msCustomers c
		ON c.Code = so_h.CustCode
	LEFT JOIN msEmployees e
		ON e.Id = so_h.SalesBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesOrderDetail
            sql = @"CREATE VIEW [Sales].[vwSalesOrderDetail]
AS
	SELECT so_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Sales.SalesOrderDetail so_d
	LEFT JOIN msItems i
		ON i.Id = so_d.ItemId
	LEFT JOIN msUoMConversions uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN msUoMs uom
		ON uom.Id = so_d.UomId
	LEFT JOIN msUoMConversions uom_c
		ON uom_c.Id = so_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesDeliveryHeader
            sql = @"CREATE VIEW [Sales].[vwSalesDeliveryHeader]
AS
	SELECT do_h.*,
		c.[Name] AS CustName,
		e.Initial AS ShippedInitial
	FROM Sales.SalesDeliveryHeader do_h
	LEFT JOIN msCustomers c
		ON c.Code = do_h.CustCode
	LEFT JOIN msEmployees e
		ON e.Id = do_h.ShippedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesDeliveryDetail
            sql = @"CREATE VIEW [Sales].[vwSalesDeliveryDetail]
AS
	SELECT do_d.*,
		ISNULL(so_d.Qty, 0) AS OrderQty,
		ISNULL(so_d.Qty, 0) - ISNULL(so_d.QtyDlv, 0) AS OutstandingQty,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Sales.SalesDeliveryDetail do_d
	LEFT JOIN Sales.SalesOrderDetail so_d
		ON so_d.Id = do_d.SODetailId
	LEFT JOIN msItems i
		ON i.Id = do_d.ItemId
	LEFT JOIN msUoMConversions uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN msUoMs uom
		ON uom.Id = do_d.UomId
	LEFT JOIN msUoMConversions uom_c
		ON uom_c.Id = do_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesInvoiceHeader
            sql = @"CREATE VIEW [Sales].[vwSalesInvoiceHeader]
AS
	SELECT si_h.*,
		c.[Name] AS CustName,
		e.Initial AS IssuedInitial
	FROM Sales.SalesInvoiceHeader si_h
	LEFT JOIN msCustomers c
		ON c.Code = si_h.CustCode
	LEFT JOIN msEmployees e
		ON e.Id = si_h.IssuedBy";
            migrationBuilder.Sql(sql);

            // Create store procedure dbo.sp_update_so_dlv_qty
            sql = @"CREATE PROCEDURE [dbo].[sp_update_so_dlv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @orderQty decimal(18, 2), @dlvQty decimal(18, 2)

	-- Get sales order data
	SELECT Code, ItemId, UnitId, Qty
	INTO #tmp_so
	FROM Sales.SalesOrderDetail so_d
	WHERE EXISTS (
		SELECT Code
		FROM Sales.SalesOrderHeader so_h
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
		AND so_h.Code = so_d.Code
	)
	
	-- Return if sales order not exists
	IF NOT EXISTS (SELECT Code FROM #tmp_so)
	BEGIN
		DROP TABLE #tmp_so
		RETURN
	END
	
	-- Get sales delivery data
	SELECT ItemId, UnitId, SUM(Qty) AS QtyDlv
	INTO #tmp_do
	FROM Sales.SalesDeliveryDetail do_d
	WHERE EXISTS (
		SELECT Code
		FROM Sales.SalesDeliveryHeader do_h
		WHERE SOCode = @code
		AND Mark <> 'V'
		AND do_h.Code = do_d.Code
	)
	GROUP BY ItemId, UnitId

	-- Return if sales delivery not exists
	IF NOT EXISTS (SELECT ItemId FROM #tmp_do)
	BEGIN
		DROP TABLE #tmp_do
		RETURN
	END

	-- Join all
	SELECT so.Code, so.ItemId, so.UnitId, so.Qty,
		ISNULL(do.QtyDlv, 0) AS QtyDlv
	INTO #tmp_all
	FROM #tmp_so so
	LEFT JOIN #tmp_do do
		ON do.ItemId = so.ItemId
		AND do.UnitId = so.UnitId

	-- Drop temp tables
	DROP TABLE #tmp_so
	DROP TABLE #tmp_do

	-- Calculate sum
	SELECT @orderQty = SUM(Qty),
		@dlvQty = SUM(QtyDlv)
	FROM #tmp_all

	-- Update SO header mark
	UPDATE Sales.SalesOrderHeader
	SET Mark = (CASE WHEN @dlvQty >= @orderQty THEN 'CMP' ELSE 'PS' END)
	WHERE Code = @code
	AND Mark NOT IN ('V', 'CLS')

	-- Update SO detail delivery qty item
	UPDATE so_d
	SET so_d.QtyDlv = #tmp_all.QtyDlv
	FROM Sales.SalesOrderDetail so_d, #tmp_all
	WHERE so_d.Code = #tmp_all.Code
	AND so_d.ItemId = #tmp_all.ItemId
	AND so_d.UnitId = #tmp_all.UnitId

	-- Drop temp table
	DROP TABLE #tmp_all

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_so') IS NOT NULL
		DROP TABLE #tmp_so
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Create store procedure dbo.sp_update_stock_mutation_from_do
            sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_do]
	@code varchar(17),
	@date date,
	@soCode varchar(17)
AS
BEGIN TRY

	DECLARE @warehouseCode varchar(8)

	SELECT @warehouseCode = WarehouseCode
	FROM Sales.SalesDeliveryHeader
	WHERE Code = @code

	SELECT *
	INTO #tmp_do
	FROM Sales.SalesDeliveryDetail
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in sales delivery item detail
	DELETE trStockMutations
	WHERE RefCode1 = @code
	AND Src = 'DO'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_do
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales delivery item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_do.ItemId,
		sm.UomId = #tmp_do.UomId,
		sm.UnitId = #tmp_do.UnitId,
		sm.Qty = #tmp_do.Qty,
		sm.RefCode2 = @soCode
	FROM trStockMutations sm, #tmp_do
	WHERE sm.RefCode1 = #tmp_do.Code
	AND sm.RefDetailId1 = #tmp_do.Id
	AND sm.Src = 'DO'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO trStockMutations
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, Code, Id, @soCode, 'DO'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM trStockMutations sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'DO'
		)

	-- Drop temp tables
	DROP TABLE #tmp_do
	
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesDeliveryDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesDeliveryHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesInvoiceDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesInvoiceHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesOrderDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesOrderHeader",
                schema: "Sales");

            migrationBuilder.RenameColumn(
                name: "CoaCOGS",
                schema: "dbo",
                table: "trPurchaseOrderDetails",
                newName: "CoaCogs");

            // Drop view dbo.vw_customers
            migrationBuilder.Sql(@"DROP VIEW dbo.vw_customers");

            // Drop view Sales.vwSalesOrderHeader
            migrationBuilder.Sql(@"DROP VIEW Sales.vwSalesOrderHeader");

            // Drop view Sales.vwSalesOrderDetail
            migrationBuilder.Sql(@"DROP VIEW Sales.vwSalesOrderDetail");

            // Drop view Sales.vwSalesDeliveryHeader
            migrationBuilder.Sql(@"DROP VIEW Sales.vwSalesDeliveryHeader");

            // Drop view Sales.vwSalesDeliveryDetail
            migrationBuilder.Sql(@"DROP VIEW Sales.vwSalesDeliveryDetail");

            // Drop view Sales.vwSalesInvoiceHeader
            migrationBuilder.Sql(@"DROP VIEW Sales.vwSalesInvoiceHeader");

            // Drop store procedure dbo.sp_update_so_dlv_qty
            migrationBuilder.Sql(@"DROP PROCEDURE dbo.sp_update_so_dlv_qty");

            // Drop store procedure dbo.sp_update_stock_mutation_from_do
            migrationBuilder.Sql(@"DROP PROCEDURE dbo.sp_update_stock_mutation_from_do");
        }
    }
}
