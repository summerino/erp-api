using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateSchemaGeneralAndInventory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_trStockMutations",
                schema: "dbo",
                table: "trStockMutations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_trPurchaseReceiveHeaders",
                schema: "dbo",
                table: "trPurchaseReceiveHeaders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_trPurchaseReceiveDetails",
                schema: "dbo",
                table: "trPurchaseReceiveDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_trPurchaseOrderHeaders",
                schema: "dbo",
                table: "trPurchaseOrderHeaders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_trPurchaseOrderDetails",
                schema: "dbo",
                table: "trPurchaseOrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msWarehouses",
                schema: "dbo",
                table: "msWarehouses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msUoMs",
                schema: "dbo",
                table: "msUoMs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msUoMConversions",
                schema: "dbo",
                table: "msUoMConversions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msTaxes",
                schema: "dbo",
                table: "msTaxes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msSupplierTypes",
                schema: "dbo",
                table: "msSupplierTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msSuppliers",
                schema: "dbo",
                table: "msSuppliers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msItems",
                schema: "dbo",
                table: "msItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msItemCategories",
                schema: "dbo",
                table: "msItemCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msEmployees",
                schema: "dbo",
                table: "msEmployees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msCustomerTypes",
                schema: "dbo",
                table: "msCustomerTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msCustomers",
                schema: "dbo",
                table: "msCustomers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msCurrencies",
                schema: "dbo",
                table: "msCurrencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msCOATypes",
                schema: "dbo",
                table: "msCOATypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_msCOAs",
                schema: "dbo",
                table: "msCOAs");

            migrationBuilder.EnsureSchema(
                name: "Accounting");

            migrationBuilder.EnsureSchema(
                name: "General");

            migrationBuilder.EnsureSchema(
                name: "Inventory");

            migrationBuilder.RenameTable(
                name: "trStockMutations",
                schema: "dbo",
                newName: "StockMutation",
                newSchema: "Inventory");

            migrationBuilder.RenameTable(
                name: "trPurchaseReceiveHeaders",
                schema: "dbo",
                newName: "PurchaseReceiveHeader",
                newSchema: "Purchasing");

            migrationBuilder.RenameTable(
                name: "trPurchaseReceiveDetails",
                schema: "dbo",
                newName: "PurchaseReceiveDetail",
                newSchema: "Purchasing");

            migrationBuilder.RenameTable(
                name: "trPurchaseOrderHeaders",
                schema: "dbo",
                newName: "PurchaseOrderHeader",
                newSchema: "Purchasing");

            migrationBuilder.RenameTable(
                name: "trPurchaseOrderDetails",
                schema: "dbo",
                newName: "PurchaseOrderDetail",
                newSchema: "Purchasing");

            migrationBuilder.RenameTable(
                name: "msWarehouses",
                schema: "dbo",
                newName: "Warehouse",
                newSchema: "Inventory");

            migrationBuilder.RenameTable(
                name: "msUoMs",
                schema: "dbo",
                newName: "UoM",
                newSchema: "Inventory");

            migrationBuilder.RenameTable(
                name: "msUoMConversions",
                schema: "dbo",
                newName: "UoMConversion",
                newSchema: "Inventory");

            migrationBuilder.RenameTable(
                name: "msTaxes",
                schema: "dbo",
                newName: "Tax",
                newSchema: "General");

            migrationBuilder.RenameTable(
                name: "msSupplierTypes",
                schema: "dbo",
                newName: "SupplierType",
                newSchema: "General");

            migrationBuilder.RenameTable(
                name: "msSuppliers",
                schema: "dbo",
                newName: "Supplier",
                newSchema: "General");

            migrationBuilder.RenameTable(
                name: "msItems",
                schema: "dbo",
                newName: "Item",
                newSchema: "Inventory");

            migrationBuilder.RenameTable(
                name: "msItemCategories",
                schema: "dbo",
                newName: "ItemCategory",
                newSchema: "Inventory");

            migrationBuilder.RenameTable(
                name: "msEmployees",
                schema: "dbo",
                newName: "Employee",
                newSchema: "General");

            migrationBuilder.RenameTable(
                name: "msCustomerTypes",
                schema: "dbo",
                newName: "CustomerType",
                newSchema: "General");

            migrationBuilder.RenameTable(
                name: "msCustomers",
                schema: "dbo",
                newName: "Customer",
                newSchema: "General");

            migrationBuilder.RenameTable(
                name: "msCurrencies",
                schema: "dbo",
                newName: "Currency",
                newSchema: "General");

            migrationBuilder.RenameTable(
                name: "msCOATypes",
                schema: "dbo",
                newName: "COAType",
                newSchema: "Accounting");

            migrationBuilder.RenameTable(
                name: "msCOAs",
                schema: "dbo",
                newName: "COA",
                newSchema: "Accounting");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockMutation",
                schema: "Inventory",
                table: "StockMutation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseReceiveHeader",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseReceiveDetail",
                schema: "Purchasing",
                table: "PurchaseReceiveDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseOrderHeader",
                schema: "Purchasing",
                table: "PurchaseOrderHeader",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseOrderDetail",
                schema: "Purchasing",
                table: "PurchaseOrderDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Warehouse",
                schema: "Inventory",
                table: "Warehouse",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UoM",
                schema: "Inventory",
                table: "UoM",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UoMConversion",
                schema: "Inventory",
                table: "UoMConversion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tax",
                schema: "General",
                table: "Tax",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupplierType",
                schema: "General",
                table: "SupplierType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Supplier",
                schema: "General",
                table: "Supplier",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Item",
                schema: "Inventory",
                table: "Item",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemCategory",
                schema: "Inventory",
                table: "ItemCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employee",
                schema: "General",
                table: "Employee",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerType",
                schema: "General",
                table: "CustomerType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Customer",
                schema: "General",
                table: "Customer",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Currency",
                schema: "General",
                table: "Currency",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_COAType",
                schema: "Accounting",
                table: "COAType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_COA",
                schema: "Accounting",
                table: "COA",
                column: "Id");

            // Drop view dbo.vw_customers
            migrationBuilder.Sql(@"DROP VIEW dbo.vw_customers");

            // Create view General.vwCustomer
            var sql = @"CREATE VIEW [General].[vwCustomer]
AS
	SELECT c.*,
		t.[Name] AS TypeName
	FROM General.Customer c
	LEFT JOIN General.CustomerType t
		ON t.Id = c.TypeId";
            migrationBuilder.Sql(sql);

            // Drop view dbo.vw_items
            migrationBuilder.Sql(@"DROP VIEW dbo.vw_items");

            // Create view Inventory.vwItem
            sql = @"CREATE VIEW [Inventory].[vwItem]
AS
	SELECT i.*,
		CASE i.TypeId
			WHEN 0 THEN 'Raw Material'
			WHEN 1 THEN 'Work In Process'
			WHEN 2 THEN 'Finished Good'
			WHEN 3 THEN 'Service'
			WHEN 4 THEN 'Consignment'
			WHEN 5 THEN 'Kits'
			WHEN 6 THEN 'Resell'
			ELSE '' END AS TypeName,
		c.[Name] AS CategoryName,
		uom.Initial AS UomInitial,
		uom_c_s.UnitEquivalent AS UomSellName,
		uom_c_b.UnitEquivalent AS UomBuyName
	FROM Inventory.Item i
	LEFT JOIN Inventory.ItemCategory c
		ON c.Id = i.CategoryId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = i.UomId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId";
            migrationBuilder.Sql(sql);

            // Drop view dbo.vw_po_h
            migrationBuilder.Sql(@"DROP VIEW dbo.vw_po_h");

            // Create view Purchasing.vwPurchaseOrderHeader
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseOrderHeader]
AS
	SELECT po_h.*,
		s.[Name] AS SupName,
		e.Initial AS RequestInitial
	FROM Purchasing.PurchaseOrderHeader po_h
	LEFT JOIN General.Supplier s
		ON s.Code = po_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = po_h.RequestBy";
            migrationBuilder.Sql(sql);

            // Drop view dbo.vw_po_d
            migrationBuilder.Sql(@"DROP VIEW dbo.vw_po_d");

            // Create view Purchasing.vwPurchaseOrderDetail
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseOrderDetail]
AS
	SELECT po_d.*,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseOrderDetail po_d
	LEFT JOIN Inventory.Item i
		ON i.Id = po_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = po_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = po_d.UnitId";
            migrationBuilder.Sql(sql);

            // Drop view dbo.vw_pr_h
            migrationBuilder.Sql(@"DROP VIEW dbo.vw_pr_h");

            // Create view Purchasing.vwPurchaseReceiveHeader
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseReceiveHeader]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ReceiveInitial
	FROM Purchasing.PurchaseReceiveHeader pr_h
	LEFT JOIN General.Supplier s
		ON s.Code = pr_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pr_h.ReceiveBy";
            migrationBuilder.Sql(sql);

            // Drop view dbo.vw_pr_d
            migrationBuilder.Sql(@"DROP VIEW dbo.vw_pr_d");

            // Create view Purchasing.vwPurchaseReceiveDetail
            sql = @"CREATE VIEW [Purchasing].[vwPurchaseReceiveDetail]
AS
	SELECT pr_d.*,
		ISNULL(po_d.Qty, 0) AS OrderQty,
		ISNULL(po_d.Qty, 0) - ISNULL(po_d.QtyRcv, 0) AS OutstandingQty,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Purchasing.PurchaseReceiveDetail  pr_d
	LEFT JOIN Purchasing.PurchaseOrderDetail po_d
		ON po_d.Id = pr_d.PODetailId
	LEFT JOIN Inventory.Item i
		ON i.Id = pr_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = pr_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = pr_d.UnitId";
            migrationBuilder.Sql(sql);

            // Create view General.vwSupplier
            sql = @"CREATE VIEW [General].[vwSupplier]
AS
	SELECT c.*,
		t.[Name] AS TypeName
	FROM General.Supplier c
	LEFT JOIN General.SupplierType t
		ON t.Id = c.TypeId";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseInvoiceHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseInvoiceHeader]
AS
	SELECT pi_h.*,
		s.[Name] AS SupName,
		e.Initial AS IssuedInitial
	FROM Purchasing.PurchaseInvoiceHeader pi_h
	LEFT JOIN General.Supplier s
		ON s.Code = pi_h.SupCode
	LEFT JOIN General.Employee e
		ON e.Id = pi_h.IssuedBy";
            migrationBuilder.Sql(sql);

            // Alter store procedure dbo.sp_update_po_rcv_qty
            sql = @"ALTER PROCEDURE [dbo].[sp_update_po_rcv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @orderQty decimal(18, 2), @rcvQty decimal(18, 2)

	-- Get purchase order data
	SELECT Code, ItemId, UnitId, Qty
	INTO #tmp_po
	FROM Purchasing.PurchaseOrderDetail po_d
	WHERE EXISTS (
		SELECT Code
		FROM Purchasing.PurchaseOrderHeader po_h
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
		AND po_h.Code = po_d.Code
	)
	AND [Type] = 0
	
	-- Return if purchase order not exists
	IF NOT EXISTS (SELECT Code FROM #tmp_po)
	BEGIN
		DROP TABLE #tmp_po
		RETURN
	END
	
	-- Get purchase receive data
	SELECT ItemId, UnitId, SUM(Qty) AS QtyRcv
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail pr_d
	WHERE EXISTS (
		SELECT Code
		FROM Purchasing.PurchaseReceiveHeader pr_h
		WHERE POCode = @code
		AND Mark <> 'V'
		AND pr_h.Code = pr_d.Code
	)
	AND [Type] = 0
	GROUP BY ItemId, UnitId

	-- Join all
	SELECT po.Code, po.ItemId, po.UnitId, po.Qty,
		ISNULL(pr.QtyRcv, 0) AS QtyRcv
	INTO #tmp_all
	FROM #tmp_po po
	LEFT JOIN #tmp_pr pr
		ON pr.ItemId = po.ItemId
		AND pr.UnitId = po.UnitId

	-- Drop temp tables
	DROP TABLE #tmp_po
	DROP TABLE #tmp_pr

	-- Calculate sum
	SELECT @orderQty = SUM(Qty),
		@rcvQty = SUM(QtyRcv)
	FROM #tmp_all

	-- Update PO header mark
	UPDATE Purchasing.PurchaseOrderHeader
	SET Mark = (
		CASE WHEN @rcvQty = 0 THEN 'A'
			WHEN @rcvQty >= @orderQty THEN 'CMP'
			ELSE 'PR' END
	)
	WHERE Code = @code
	AND Mark NOT IN ('V', 'CLS')

	-- Update PO detail receive qty item
	UPDATE po_d
	SET po_d.QtyRcv = #tmp_all.QtyRcv
	FROM Purchasing.PurchaseOrderDetail po_d, #tmp_all
	WHERE po_d.Code = #tmp_all.Code
	AND po_d.ItemId = #tmp_all.ItemId
	AND po_d.UnitId = #tmp_all.UnitId

	-- Drop temp table
	DROP TABLE #tmp_all

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_po') IS NOT NULL
		DROP TABLE #tmp_po
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Alter store procedure dbo.sp_update_so_dlv_qty
            sql = @"ALTER PROCEDURE [dbo].[sp_update_so_dlv_qty]
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
	SET Mark = (
		CASE WHEN @dlvQty = 0 THEN 'A'
			WHEN @dlvQty >= @orderQty THEN 'CMP'
			ELSE 'PS' END
	)
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

            // Alter store procedure dbo.sp_update_stock_mutation_from_do
            sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_do]
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
	DELETE Inventory.StockMutation
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
	FROM Inventory.StockMutation sm, #tmp_do
	WHERE sm.RefCode1 = #tmp_do.Code
	AND sm.RefDetailId1 = #tmp_do.Id
	AND sm.Src = 'DO'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, Code, Id, @soCode, 'DO'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
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

            // Alter store procedure dbo.sp_update_stock_mutation_from_rcv
            sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_rcv]
	@code varchar(17),
	@date date,
	@poCode varchar(17)
AS
BEGIN TRY

	SELECT *
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in purchase receive item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'RCV'
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
		sm.RefCode2 = CASE WHEN [Type] = 0 THEN @poCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'RCV'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, Code, Id,
			CASE WHEN [Type] = 0 THEN @poCode ELSE NULL END,
			'RCV'
		FROM #tmp_pr pr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = pr.Code
			AND sm.RefDetailId1 = pr.Id
			AND sm.Src = 'RCV'
		)

	-- Drop temp tables
	DROP TABLE #tmp_pr
	
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Warehouse",
                schema: "Inventory",
                table: "Warehouse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UoMConversion",
                schema: "Inventory",
                table: "UoMConversion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UoM",
                schema: "Inventory",
                table: "UoM");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tax",
                schema: "General",
                table: "Tax");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupplierType",
                schema: "General",
                table: "SupplierType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Supplier",
                schema: "General",
                table: "Supplier");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockMutation",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseReceiveHeader",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseReceiveDetail",
                schema: "Purchasing",
                table: "PurchaseReceiveDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseOrderHeader",
                schema: "Purchasing",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseOrderDetail",
                schema: "Purchasing",
                table: "PurchaseOrderDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemCategory",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Item",
                schema: "Inventory",
                table: "Item");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employee",
                schema: "General",
                table: "Employee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerType",
                schema: "General",
                table: "CustomerType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Customer",
                schema: "General",
                table: "Customer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Currency",
                schema: "General",
                table: "Currency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_COAType",
                schema: "Accounting",
                table: "COAType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_COA",
                schema: "Accounting",
                table: "COA");

            migrationBuilder.RenameTable(
                name: "Warehouse",
                schema: "Inventory",
                newName: "msWarehouses",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "UoMConversion",
                schema: "Inventory",
                newName: "msUoMConversions",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "UoM",
                schema: "Inventory",
                newName: "msUoMs",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Tax",
                schema: "General",
                newName: "msTaxes",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "SupplierType",
                schema: "General",
                newName: "msSupplierTypes",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Supplier",
                schema: "General",
                newName: "msSuppliers",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "StockMutation",
                schema: "Inventory",
                newName: "trStockMutations",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseReceiveHeader",
                schema: "Purchasing",
                newName: "trPurchaseReceiveHeaders",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseReceiveDetail",
                schema: "Purchasing",
                newName: "trPurchaseReceiveDetails",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseOrderHeader",
                schema: "Purchasing",
                newName: "trPurchaseOrderHeaders",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "PurchaseOrderDetail",
                schema: "Purchasing",
                newName: "trPurchaseOrderDetails",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "ItemCategory",
                schema: "Inventory",
                newName: "msItemCategories",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Item",
                schema: "Inventory",
                newName: "msItems",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Employee",
                schema: "General",
                newName: "msEmployees",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "CustomerType",
                schema: "General",
                newName: "msCustomerTypes",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Customer",
                schema: "General",
                newName: "msCustomers",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "Currency",
                schema: "General",
                newName: "msCurrencies",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "COAType",
                schema: "Accounting",
                newName: "msCOATypes",
                newSchema: "dbo");

            migrationBuilder.RenameTable(
                name: "COA",
                schema: "Accounting",
                newName: "msCOAs",
                newSchema: "dbo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msWarehouses",
                schema: "dbo",
                table: "msWarehouses",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msUoMConversions",
                schema: "dbo",
                table: "msUoMConversions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msUoMs",
                schema: "dbo",
                table: "msUoMs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msTaxes",
                schema: "dbo",
                table: "msTaxes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msSupplierTypes",
                schema: "dbo",
                table: "msSupplierTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msSuppliers",
                schema: "dbo",
                table: "msSuppliers",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_trStockMutations",
                schema: "dbo",
                table: "trStockMutations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_trPurchaseReceiveHeaders",
                schema: "dbo",
                table: "trPurchaseReceiveHeaders",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_trPurchaseReceiveDetails",
                schema: "dbo",
                table: "trPurchaseReceiveDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_trPurchaseOrderHeaders",
                schema: "dbo",
                table: "trPurchaseOrderHeaders",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_trPurchaseOrderDetails",
                schema: "dbo",
                table: "trPurchaseOrderDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msItemCategories",
                schema: "dbo",
                table: "msItemCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msItems",
                schema: "dbo",
                table: "msItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msEmployees",
                schema: "dbo",
                table: "msEmployees",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msCustomerTypes",
                schema: "dbo",
                table: "msCustomerTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msCustomers",
                schema: "dbo",
                table: "msCustomers",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msCurrencies",
                schema: "dbo",
                table: "msCurrencies",
                column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msCOATypes",
                schema: "dbo",
                table: "msCOATypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_msCOAs",
                schema: "dbo",
                table: "msCOAs",
                column: "Id");

            // Create view dbo.vw_customers
            var sql = @"CREATE VIEW [dbo].[vw_customers]
AS
	SELECT c.*,
		t.[Name] AS TypeName
	FROM msCustomers c
	LEFT JOIN msCustomerTypes t
		ON t.Id = c.TypeId";
            migrationBuilder.Sql(sql);

            // Drop view General.vwCustomer
            migrationBuilder.Sql(@"DROP VIEW General.vwCustomer");

            // Create view dbo.vw_items
            sql = @"CREATE VIEW [dbo].[vw_items]
AS
	SELECT i.*,
		CASE i.TypeId
			WHEN 0 THEN 'Raw Material'
			WHEN 1 THEN 'Work In Process'
			WHEN 2 THEN 'Finished Good'
			WHEN 3 THEN 'Service'
			WHEN 4 THEN 'Consignment'
			WHEN 5 THEN 'Kits'
			WHEN 6 THEN 'Resell'
			ELSE '' END AS TypeName,
		c.[Name] AS CategoryName,
		uom.Initial AS UomInitial,
		uom_c_s.UnitEquivalent AS UomSellName,
		uom_c_b.UnitEquivalent AS UomBuyName
	FROM msItems i
	LEFT JOIN msItemCategories c
		ON c.Id = i.CategoryId
	LEFT JOIN msUoMs uom
		ON uom.Id = i.UomId
	LEFT JOIN msUoMConversions uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN msUoMConversions uom_c_b
		ON uom_c_b.Id = i.UomBuyId";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwItem
            migrationBuilder.Sql(@"DROP VIEW Inventory.vwItem");

            // Create view dbo.vw_po_h
            sql = @"CREATE VIEW [dbo].[vw_po_h]
AS
	SELECT po_h.*,
		s.[Name] AS SupName,
		e.Initial AS RequestInitial
	FROM trPurchaseOrderHeaders po_h
	LEFT JOIN msSuppliers s
		ON s.Code = po_h.SupCode
	LEFT JOIN msEmployees e
		ON e.Id = po_h.RequestBy";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseOrderHeader
            migrationBuilder.Sql(@"DROP VIEW Purchasing.vwPurchaseOrderHeader");

            // Create view dbo.vw_po_d
            sql = @"CREATE VIEW [dbo].[vw_po_d]
AS
	SELECT po_d.*,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM trPurchaseOrderDetails po_d
	LEFT JOIN msItems i
		ON i.Id = po_d.ItemId
	LEFT JOIN msUoMConversions uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN msUoMs uom
		ON uom.Id = po_d.UomId
	LEFT JOIN msUoMConversions uom_c
		ON uom_c.Id = po_d.UnitId";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseOrderDetail
            migrationBuilder.Sql(@"DROP VIEW Purchasing.vwPurchaseOrderDetail");

            // Create view dbo.vw_pr_h
            sql = @"CREATE VIEW [dbo].[vw_pr_h]
AS
	SELECT pr_h.*,
		s.[Name] AS SupName,
		e.Initial AS ReceiveInitial
	FROM trPurchaseReceiveHeaders pr_h
	LEFT JOIN msSuppliers s
		ON s.Code = pr_h.SupCode
	LEFT JOIN msEmployees e
		ON e.Id = pr_h.ReceiveBy";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseReceiveHeader
            migrationBuilder.Sql(@"DROP VIEW Purchasing.vwPurchaseReceiveHeader");

            // Create view dbo.vw_pr_d
            sql = @"CREATE VIEW [dbo].[vw_pr_d]
AS
	SELECT pr_d.*,
		ISNULL(po_d.Qty, 0) AS OrderQty,
		ISNULL(po_d.Qty, 0) - ISNULL(po_d.QtyRcv, 0) AS OutstandingQty,
		i.[Name] AS ItemName,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM trPurchaseReceiveDetails pr_d
	LEFT JOIN trPurchaseOrderDetails po_d
		ON po_d.Id = pr_d.PODetailId
	LEFT JOIN msItems i
		ON i.Id = pr_d.ItemId
	LEFT JOIN msUoMConversions uom_c_b
		ON uom_c_b.Id = i.UomBuyId
	LEFT JOIN msUoMs uom
		ON uom.Id = pr_d.UomId
	LEFT JOIN msUoMConversions uom_c
		ON uom_c.Id = pr_d.UnitId";
            migrationBuilder.Sql(sql);

            // Drop view Purchasing.vwPurchaseReceiveDetail
            migrationBuilder.Sql(@"DROP VIEW Purchasing.vwPurchaseReceiveDetail");

            // Drop view General.vwSupplier
            migrationBuilder.Sql(@"DROP VIEW General.vwSupplier");
        }
    }
}
