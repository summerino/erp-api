using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateTableTransferStockAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NoTax",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TaxInvoiceDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxInvoiceNo",
                schema: "Purchasing",
                table: "PurchaseReturnHeader",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Inventory",
                table: "ItemGroup",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Initial",
                schema: "Inventory",
                table: "ItemGroup",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.CreateTable(
                name: "DeliveryPlanDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    TransCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    IsFailShipment = table.Column<bool>(type: "bit", nullable: false),
                    NotesFailShipment = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPlanDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryPlanHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    SrcTrans = table.Column<short>(type: "smallint", nullable: false),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<long>(type: "bigint", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    TotalVolume = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TotalWeight = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    TotalVehicleVolume = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalVehicleWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_DeliveryPlanHeader", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryPlanUndeliveredItem",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    DlvPlanDetailId = table.Column<long>(type: "bigint", nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPlanUndeliveredItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransferStockDetail",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferStockDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransferStockHeader",
                schema: "Inventory",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    OriginTransferCode = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    WarehouseCodeFrom = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    WarehouseCodeTo = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
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
                    table.PrimaryKey("PK_TransferStockHeader", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemParameter_Code",
                schema: "SystemManagement",
                table: "SystemParameter",
                column: "Code",
                unique: true);

            // Reorder column Purchasing.PurchaseReturnHeader
            var sql = @"BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Purchasing.Tmp_PurchaseReturnHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	RcvCode varchar(17) NULL,
	RefNo varchar(30) NULL,
	Type smallint NOT NULL,
	SupCode varchar(8) NOT NULL,
	ShippedBy bigint NOT NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	NoTax bit NOT NULL,
	IncludeTax bit NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	TaxInvoiceDate date NULL,
	TaxInvoiceNo varchar(50) NULL,
	Notes varchar(256) NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Purchasing.Tmp_PurchaseReturnHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Purchasing.PurchaseReturnHeader)
	 EXEC('INSERT INTO Purchasing.Tmp_PurchaseReturnHeader (Code, Date, RcvCode, RefNo, Type, SupCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDisc, NoTax, IncludeTax, TaxAmount, Total, DPP, TaxInvoiceDate, TaxInvoiceNo, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, RcvCode, RefNo, Type, SupCode, ShippedBy, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDisc, NoTax, IncludeTax, TaxAmount, Total, DPP, TaxInvoiceDate, TaxInvoiceNo, Notes, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Purchasing.PurchaseReturnHeader WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Purchasing.PurchaseReturnHeader
GO
EXECUTE sp_rename N'Purchasing.Tmp_PurchaseReturnHeader', N'PurchaseReturnHeader', 'OBJECT' 
GO
ALTER TABLE Purchasing.PurchaseReturnHeader ADD CONSTRAINT
	PK_PurchaseReturnHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Purchasing.vwPurchaseReturnHeader
            sql = @"exec sp_refreshview 'Purchasing.vwPurchaseReturnHeader'";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwTransferStockHeader
            sql = @"CREATE VIEW [Inventory].[vwTransferStockHeader]
AS
	SELECT ts_h.*,
		w_f.Initial AS WarehouseInitialFrom,
		w_t.Initial AS WarehouseInitialTo,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE ts_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Inventory.TransferStockHeader ts_h
	LEFT JOIN Inventory.Warehouse w_f
		ON w_f.Code = ts_h.WarehouseCodeFrom
	LEFT JOIN Inventory.Warehouse w_t
		ON w_t.Code = ts_h.WarehouseCodeTo
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = ts_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = ts_h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = ts_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwTransferStockDetail
            sql = @"CREATE VIEW [Inventory].[vwTransferStockDetail]
AS
	SELECT ts_d.*,
		i.[Name] AS ItemName
	FROM Inventory.TransferStockDetail ts_d
	LEFT JOIN Inventory.Item i
		ON i.Id = ts_d.ItemId";
            migrationBuilder.Sql(sql);

            // Alter view General.vwVehicle
            sql = @"ALTER VIEW [General].[vwVehicle]
AS
    SELECT v.*,
        t.[Name] AS TypeName,
        e.Initial AS DriverInitial,
		u.Initial AS UpdatedInitial
    FROM General.Vehicle v
    LEFT JOIN General.VehicleType t
        ON t.Id = v.TypeId
    LEFT JOIN General.Employee e
        ON e.Id = v.DriverId
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = v.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Alter view Inventory.vwItem
            sql = @"ALTER VIEW [Inventory].[vwItem]
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
        uom_c_b.UnitEquivalent AS UomBuyName,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        wq.WarehouseCode,
        ISNULL(wq.QtyOnHand, 0) As QtyOnHand
    FROM Inventory.Item i
    LEFT JOIN Inventory.ItemCategory c
        ON c.Id = i.CategoryId
    LEFT JOIN Inventory.UoM uom
        ON uom.Id = i.UomId
    LEFT JOIN Inventory.UoMConversion uom_c_s
        ON uom_c_s.Id = i.UomSellId
    LEFT JOIN Inventory.UoMConversion uom_c_b
        ON uom_c_b.Id = i.UomBuyId
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = c.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = c.UpdatedBy
    LEFT JOIN Inventory.WarehouseQuantity wq
        ON wq.ItemId = i.Id";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseInvoiceHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseInvoiceHeader]
AS
    SELECT pi_h.*,
        s.[Name] AS SupName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE pi_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Purchasing.PurchaseInvoiceHeader pi_h
    LEFT JOIN General.Supplier s
        ON s.Code = pi_h.SupCode
    LEFT JOIN General.Employee e
        ON e.Id = pi_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = pi_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = pi_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = pi_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Alter view Purchasing.vwPurchaseOrderHeader
            sql = @"ALTER VIEW [Purchasing].[vwPurchaseOrderHeader]
AS
    SELECT po_h.*,
        s.[Name] AS SupName,
        e.Initial AS RequestInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE po_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'PR' THEN 'Partial Received'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'CLS' THEN 'Closed' END AS [Status]
    FROM Purchasing.PurchaseOrderHeader po_h
    LEFT JOIN General.Supplier s
        ON s.Code = po_h.SupCode
    LEFT JOIN General.Employee e
        ON e.Id = po_h.RequestBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = po_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = po_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = po_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwSalesInvoiceHeader
            sql = @"ALTER VIEW [Sales].[vwSalesInvoiceHeader]
AS
    SELECT si_h.*,
        c.[Name] AS CustName,
        e.Initial AS IssuedInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE si_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status]
    FROM Sales.SalesInvoiceHeader si_h
    LEFT JOIN General.Customer c
        ON c.Code = si_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = si_h.IssuedBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = si_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = si_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = si_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Alter view Sales.vwSalesOrderHeader
            sql = @"ALTER VIEW [Sales].[vwSalesOrderHeader]
AS
    SELECT so_h.*,
        c.[Name] AS CustName,
        e.Initial AS SalesInitial,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE so_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'PS' THEN 'Partial Shipped'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'CLS' THEN 'Closed' END AS [Status]
    FROM Sales.SalesOrderHeader so_h
    LEFT JOIN General.Customer c
        ON c.Code = so_h.CustCode
    LEFT JOIN General.Employee e
        ON e.Id = so_h.SalesBy
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = so_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = so_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = so_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwItemCategory
            sql = @"CREATE VIEW [Inventory].[vwItemCategory]
AS
    SELECT ic.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial
    FROM Inventory.ItemCategory ic
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = ic.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = ic.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Inventory.vwItemGroup
            sql = @"CREATE VIEW [Inventory].[vwItemGroup]
AS
    SELECT ig.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial
    FROM Inventory.ItemGroup ig
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = ig.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = ig.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwArea
            sql = @"CREATE VIEW [Sales].[vwArea]
AS
    SELECT a.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial
    FROM Sales.Area a
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = a.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = a.UpdatedBy";
            migrationBuilder.Sql(sql);

            // Create view Sales.vwDeliveryPlanHeader
            sql = @"CREATE VIEW [Sales].[vwDeliveryPlanHeader]
AS
	SELECT dp_h.*,
		v.VehicleNo,
		e.Initial AS DriverInitial,
		w.Initial AS WarehouseInitial,
		u_c.Initial AS CreatedInitial,
		u_u.Initial AS UpdatedInitial,
		u_a.Initial AS ApprovedInitial,
		CASE dp_h.Mark
			WHEN 'A' THEN 'Active'
			WHEN 'V' THEN 'Void' END AS [Status]
	FROM Sales.DeliveryPlanHeader dp_h
	LEFT JOIN General.Vehicle v
		ON v.Id = dp_h.VehicleId
	LEFT JOIN General.Employee e
		ON e.Id = dp_h.DriverId
	LEFT JOIN Inventory.Warehouse w
		ON w.Code = dp_h.WarehouseCode
	LEFT JOIN SystemManagement.[User] u_c
		ON u_c.Id = dp_h.CreatedBy
	LEFT JOIN SystemManagement.[User] u_u
		ON u_u.Id = dp_h.UpdatedBy
	LEFT JOIN SystemManagement.[User] u_a
		ON u_a.Id = dp_h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Alter procedure dbo.sp_update_stock_mutation_from_do
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

	SELECT do_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN do_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN do_d.Qty
			ELSE do_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_do
	FROM Sales.SalesDeliveryDetail do_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = do_d.UomId
		AND uom_c.Id = do_d.UnitId
	WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'DO'

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
		sm.BaseUnit = #tmp_do.BaseUnit,
		sm.BaseQty = #tmp_do.BaseQty,
		sm.RefCode2 = @soCode
	FROM Inventory.StockMutation sm, #tmp_do
	WHERE sm.RefCode1 = #tmp_do.Code
	AND sm.RefDetailId1 = #tmp_do.Id
	AND sm.Src = 'DO'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, Code, BaseUnit, BaseQty, Id, @soCode, 'DO'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'DO'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId,
        SUM(CASE WHEN Src IN ('RCV', 'SR','ADJ') THEN BaseQty ELSE -BaseQty END) AS Qty
    INTO #tmp_wq
    FROM Inventory.StockMutation sm
	WHERE EXISTS (
        SELECT WarehouseCode,ItemId
        FROM #tmp_ori_sm ori
        WHERE ori.WarehouseCode = sm.WarehouseCode
        AND ori.ItemId = sm.ItemId
        UNION
        SELECT WarehouseCode,ItemId
        FROM Inventory.StockMutation sm_1
        WHERE sm_1.RefCode1 = @code
        AND sm_1.WarehouseCode = sm.WarehouseCode
        AND sm_1.ItemId = sm.ItemId
    )
    GROUP BY WarehouseCode, ItemId
	
	CREATE NONCLUSTERED INDEX IX1 ON #tmp_wq (WarehouseCode,ItemId)

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = Qty FROM #tmp_wq

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand = @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
		ELSE
		BEGIN
			INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
			VALUES (@WHId,@ItemId,@Qty,0,0,0,0,GETDATE())
		END

		DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
	END

	-- Drop temp tables
	DROP TABLE #tmp_do
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Alter procedure dbo.sp_update_stock_mutation_from_rcv
            sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_rcv]
	@code varchar(17),
	@date date,
	@poCode varchar(17)
AS
BEGIN TRY

	SELECT pr_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN pr_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN pr_d.Qty
			ELSE pr_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail pr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = pr_d.UomId
		AND uom_c.Id = pr_d.UnitId
	WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'RCV'

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
		sm.BaseUnit = #tmp_pr.BaseUnit,
		sm.BaseQty = #tmp_pr.BaseQty,
		sm.RefCode2 = CASE WHEN [Type] = 0 THEN @poCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'RCV'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty,
			Code, Id,
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

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId,
        SUM(CASE WHEN Src IN ('RCV', 'SR','ADJ') THEN BaseQty ELSE -BaseQty END) AS Qty
    INTO #tmp_wq
    FROM Inventory.StockMutation sm
	WHERE EXISTS (
        SELECT WarehouseCode,ItemId
        FROM #tmp_ori_sm ori
        WHERE ori.WarehouseCode = sm.WarehouseCode
        AND ori.ItemId = sm.ItemId
        UNION
        SELECT WarehouseCode,ItemId
        FROM Inventory.StockMutation sm_1
        WHERE sm_1.RefCode1 = @code
        AND sm_1.WarehouseCode = sm.WarehouseCode
        AND sm_1.ItemId = sm.ItemId
    )
    GROUP BY WarehouseCode, ItemId
	
	CREATE NONCLUSTERED INDEX IX1 ON #tmp_wq (WarehouseCode,ItemId)

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = Qty FROM #tmp_wq

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand = @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
		ELSE
		BEGIN
			INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
			VALUES (@WHId,@ItemId,@Qty,0,0,0,0,GETDATE())
		END

		DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
	END
		
	-- Drop temp tables
	DROP TABLE #tmp_pr
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryPlanDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "DeliveryPlanHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "DeliveryPlanUndeliveredItem",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "TransferStockDetail",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "TransferStockHeader",
                schema: "Inventory");

            migrationBuilder.DropIndex(
                name: "IX_SystemParameter_Code",
                schema: "SystemManagement",
                table: "SystemParameter");

            migrationBuilder.DropColumn(
                name: "NoTax",
                schema: "Purchasing",
                table: "PurchaseReturnHeader");

            migrationBuilder.DropColumn(
                name: "TaxInvoiceDate",
                schema: "Purchasing",
                table: "PurchaseReturnHeader");

            migrationBuilder.DropColumn(
                name: "TaxInvoiceNo",
                schema: "Purchasing",
                table: "PurchaseReturnHeader");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Inventory",
                table: "ItemGroup",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Initial",
                schema: "Inventory",
                table: "ItemGroup",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            // Drop view Inventory.vwTransferStockHeader
            var sql = @"DROP VIEW [Inventory].[vwTransferStockHeader]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwTransferStockDetail
            sql = @"DROP VIEW [Inventory].[vwTransferStockDetail]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwItemCategory
            sql = @"DROP VIEW [Inventory].[vwItemCategory]";
            migrationBuilder.Sql(sql);

            // Drop view Inventory.vwItemGroup
            sql = @"DROP VIEW [Inventory].[vwItemGroup]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwArea
            sql = @"DROP VIEW [Sales].[vwArea]";
            migrationBuilder.Sql(sql);

            // Drop view Sales.vwDeliveryPlanHeader
            sql = @"DROP VIEW [Sales].[vwDeliveryPlanHeader]";
            migrationBuilder.Sql(sql);
        }
    }
}
