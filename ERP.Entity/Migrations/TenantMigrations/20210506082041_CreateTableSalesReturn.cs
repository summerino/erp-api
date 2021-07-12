using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableSalesReturn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproveBy",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropColumn(
                name: "ApproveBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader");

            migrationBuilder.DropColumn(
                name: "ApproveBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.DropColumn(
                name: "IsLowestLevel",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Inventory",
                table: "ItemCategory",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Initial",
                schema: "Inventory",
                table: "ItemCategory",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "datetime",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "Inventory",
                table: "AdjustmentDetail",
                type: "varchar(17)",
                unicode: false,
                maxLength: 17,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(17)",
                oldUnicode: false,
                oldMaxLength: 17,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaseQtyOnTransfer",
                schema: "Inventory",
                table: "AdjustmentDetail",
                type: "decimal(18,2)",
                nullable: false);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyOnTransfer",
                schema: "Inventory",
                table: "AdjustmentDetail",
                type: "decimal(19,6)",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "Area",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Deep = table.Column<int>(type: "int", nullable: true),
                    Lineage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Area", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransDetailId = table.Column<long>(type: "bigint", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QtyDlv = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_SalesReturnDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetailExchDiffItem",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ReturnDetailId = table.Column<long>(type: "bigint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TaxId = table.Column<int>(type: "int", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NettPrice = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(19,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnDetailExchDiffItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    SalesBy = table.Column<long>(type: "bigint", nullable: false),
                    CurrCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShipmentFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HandlingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalDisc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NoTax = table.Column<bool>(type: "bit", nullable: false),
                    IncludeTax = table.Column<bool>(type: "bit", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DPP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxInvoiceNo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TaxInvoiceDate = table.Column<DateTime>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
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
                    table.PrimaryKey("PK_SalesReturnHeader", x => x.Code);
                });

            // Create view Sales.vwSalesReturnHeader
            var sql = @"CREATE VIEW [Sales].[vwSalesReturnHeader]
AS
	SELECT sr_h.*,
		c.[Name] AS CustName,
		e.Initial AS SalesInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE sr_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void'
			WHEN 'PS' THEN 'Partial Shipped'
			WHEN 'CMP' THEN 'Completed'
			WHEN 'CLS' THEN 'Closed' END AS [Status]
	FROM Sales.SalesReturnHeader sr_h
	LEFT JOIN General.Customer c
		ON c.Code = sr_h.CustCode
	LEFT JOIN General.Employee e
		ON e.Id = sr_h.SalesBy
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = sr_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = sr_h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = sr_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwSalesReturnDetail
            sql = @"CREATE VIEW [Sales].[vwSalesReturnDetail]
AS
	SELECT sr_d.*,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Sales.SalesReturnDetail sr_d
	LEFT JOIN Inventory.Item i
		ON i.Id = sr_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = sr_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = sr_d.UnitId";
            migrationBuilder.Sql(sql);

            // Alter view Inventory.vwAdjustmentHeader
            sql = @"ALTER VIEW [Inventory].[vwAdjustmentHeader]
AS
	SELECT adj_h.*,
		w.Initial AS WarehouseInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE adj_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Inventory.AdjustmentHeader adj_h
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = adj_h.WarehouseCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = adj_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = adj_h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = adj_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create store procedure dbo.sp_update_stock_mutation_from_pr
            sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_pr]
	@code varchar(17),
	@date date,
	@rcvCode varchar(17)
AS
BEGIN TRY

	SELECT *
	INTO #tmp_pr
	FROM Purchasing.PurchaseReturnDetail
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in purchase receive item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'RTN'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_pr
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in purchase receive item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_pr.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_pr.ItemId,
		sm.UomId = #tmp_pr.UomId,
		sm.UnitId = #tmp_pr.UnitId,
		sm.Qty = #tmp_pr.Qty,
		sm.RefCode2 = CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'RTN'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, 0, Code, Id,
			CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END,
			'RTN'
		FROM #tmp_pr pr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = pr.Code
			AND sm.RefDetailId1 = pr.Id
			AND sm.Src = 'RTN'
		)

	-- Drop temp tables
	DROP TABLE #tmp_pr

	-- Update BaseUnit & BaseQty
	SELECT *
	INTO #tmp_stk
	FROM Inventory.StockMutation WHERE RefCode1 = @code
	
	DECLARE @Id int
	DECLARE @UomId int
	DECLARE @UnitId int
	DECLARE @IsBaseUnit int
	DECLARE @Seq int

	WHILE EXISTS(SELECT * FROM #tmp_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId FROM #tmp_stk
		SELECT @IsBaseUnit = IsBaseUnit, @Seq = Seq FROM Inventory.UoMConversion WHERE UomId = @UomId AND Id = @UnitId

		IF @IsBaseUnit = 1 
		BEGIN
			UPDATE Inventory.StockMutation SET BaseQty = Qty,BaseUnit = UnitId WHERE Id = @Id
		END
		ELSE
		BEGIN
			UPDATE Inventory.StockMutation SET BaseQty = Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
			BaseUnit = (SELECT Id FROM Inventory.UoMConversion WHERE UomId = @UomId AND IsBaseUnit = 1) WHERE Id = @Id
		END

		DELETE #tmp_stk WHERE Id = @Id
	END

	-- Drop temp tables
	DROP TABLE #tmp_stk

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, SUM(BaseQty) as Qty
	INTO #tmp_nrtn
	FROM Inventory.StockMutation WHERE Src != 'RTN' GROUP BY WarehouseCode, ItemId
	
	SELECT WarehouseCode, ItemId, SUM(BaseQty) as Qty
	INTO #tmp_rtn
	FROM Inventory.StockMutation WHERE Src = 'RTN' GROUP BY WarehouseCode, ItemId

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @FQty decimal

	WHILE EXISTS(SELECT * FROM #tmp_nrtn)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId FROM #tmp_nrtn

		SELECT @Qty = Qty FROM #tmp_rtn WHERE WarehouseCode = @WHId AND ItemId = @ItemId

		IF EXISTS(SELECT * FROM #tmp_rtn WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE #tmp_nrtn SET Qty -= @Qty WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END

		SELECT @FQty = Qty FROM #tmp_nrtn WHERE WarehouseCode = @WHId AND ItemId = @ItemId

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand = @FQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
		ELSE
		BEGIN
			INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
			VALUES (@WHId,@ItemId,@FQty,0,0,0,0,GETDATE())
		END

		DELETE #tmp_nrtn WHERE WarehouseCode = @WHId AND ItemId = @ItemId
	END

	-- Drop temp tables
	DROP TABLE #tmp_nrtn
	DROP TABLE #tmp_rtn

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_stk') IS NOT NULL
		DROP TABLE #tmp_stk
	IF OBJECT_ID('tempdb.dbo.#tmp_nrtn') IS NOT NULL
		DROP TABLE #tmp_nrtn
	IF OBJECT_ID('tempdb.dbo.#tmp_rtn') IS NOT NULL
		DROP TABLE #tmp_rtn

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Alter view Inventory.vwAdjustmentDetail
            sql = @"ALTER VIEW [Inventory].[vwAdjustmentDetail]
AS
	SELECT adj_d.*,
		adj_d.QtyOnHand + adj_d.QtyAdjust AS QtyOpname,
		adj_d.QtyAdjust AS Different,
		i.[Name] AS ItemName
	FROM Inventory.AdjustmentDetail adj_d
	LEFT JOIN Inventory.Item i
		ON i.Id = adj_d.ItemId";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Purchasing.VwPurchaseOrderHeader
            sql = @"execute sp_refreshview 'Purchasing.VwPurchaseOrderHeader'";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Purchasing.vwPurchaseReceiveHeader
            sql = @"execute sp_refreshview 'Purchasing.vwPurchaseReceiveHeader'";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Purchasing.vwPurchaseReturnHeader
            sql = @"execute sp_refreshview 'Purchasing.vwPurchaseReturnHeader'";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Purchasing.vwPurchaseInvoiceHeader
            sql = @"execute sp_refreshview 'Purchasing.vwPurchaseInvoiceHeader'";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Sales.vwSalesOrderHeader
            sql = @"execute sp_refreshview 'Sales.vwSalesOrderHeader'";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Sales.vwSalesDeliveryHeader
            sql = @"execute sp_refreshview 'Sales.vwSalesDeliveryHeader'";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Sales.vwSalesInvoiceHeader
            sql = @"execute sp_refreshview 'Sales.vwSalesInvoiceHeader'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Area",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesReturnDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesReturnDetailExchDiffItem",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesReturnHeader",
                schema: "Sales");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                schema: "Purchasing",
                table: "PurchaseInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                schema: "Inventory",
                table: "AdjustmentHeader");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                schema: "Inventory",
                table: "AdjustmentHeader");

            migrationBuilder.DropColumn(
                name: "BaseQtyOnTransfer",
                schema: "Inventory",
                table: "AdjustmentDetail");

            migrationBuilder.DropColumn(
                name: "QtyOnTransfer",
                schema: "Inventory",
                table: "AdjustmentDetail");

            migrationBuilder.AddColumn<long>(
                name: "ApproveBy",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ApproveBy",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ApproveBy",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Inventory",
                table: "ItemCategory",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Initial",
                schema: "Inventory",
                table: "ItemCategory",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "IsLowestLevel",
                schema: "Inventory",
                table: "ItemCategory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Mark",
                schema: "Inventory",
                table: "AdjustmentHeader",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "Inventory",
                table: "AdjustmentDetail",
                type: "varchar(17)",
                unicode: false,
                maxLength: 17,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(17)",
                oldUnicode: false,
                oldMaxLength: 17);

            // Drop view Sales.vwSalesReturnHeader
            var sql = @"DROP VIEW [Sales].[vwSalesReturnHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwSalesReturnDetail
            sql = @"DROP VIEW [Sales].[vwSalesReturnDetail]";
            migrationBuilder.Sql(sql);

            // Drop store procedure dbo.sp_update_stock_mutation_from_pr
            sql = @"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_pr]";
            migrationBuilder.Sql(sql);
        }
    }
}
