using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateTableSalesmanGroupAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesmanGroup",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SupervisorId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesmanGroup", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_RefCode1",
                schema: "Inventory",
                table: "StockMutation",
                column: "RefCode1");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_RefCode2",
                schema: "Inventory",
                table: "StockMutation",
                column: "RefCode2");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_RefDetailId1",
                schema: "Inventory",
                table: "StockMutation",
                column: "RefDetailId1");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_Src",
                schema: "Inventory",
                table: "StockMutation",
                column: "Src");

            migrationBuilder.CreateIndex(
                name: "IX_StockMutation_Type",
                schema: "Inventory",
                table: "StockMutation",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_SalesDeliveryHeader_TransCode",
                schema: "Sales",
                table: "SalesDeliveryHeader",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReceiveHeader_TransCode",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPlanDetail_TransCode",
                schema: "Sales",
                table: "DeliveryPlanDetail",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_DebitMemo_TransCode",
                schema: "Purchasing",
                table: "DebitMemo",
                column: "TransCode");

            migrationBuilder.CreateIndex(
                name: "IX_CreditMemo_TransCode",
                schema: "Sales",
                table: "CreditMemo",
                column: "TransCode");

            // Create view Sales.vwSalesmanGroup
            var sql = @"CREATE VIEW [Sales].[vwSalesmanGroup]
AS
    SELECT sg.*,
        e.Initial AS SupervisorInitial,
		u.Initial AS UpdatedInitial
    FROM Sales.SalesmanGroup sg
    LEFT JOIN General.Employee e
        ON e.Id = sg.SupervisorId
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = sg.UpdatedBy";
            migrationBuilder.Sql(sql);

			// Create procedure dbo.sp_update_stock_mutation_from_po
			sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_po]
	@code varchar(17),
	@date date
AS
BEGIN TRY
	DECLARE @warehouseCode varchar(8)

	SELECT @warehouseCode = WarehouseCode
	FROM Purchasing.PurchaseOrderHeader
	WHERE Code = @code

	SELECT po_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN po_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN po_d.Qty
			ELSE po_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_po
	FROM Purchasing.PurchaseOrderDetail po_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = po_d.UomId
		AND uom_c.Id = po_d.UnitId
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in purchase order item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PO'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_po
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in purchase order item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_po.ItemId,
		sm.UomId = #tmp_po.UomId,
		sm.UnitId = #tmp_po.UnitId,
		sm.Qty = #tmp_po.Qty,
		sm.BaseUnit = #tmp_po.BaseUnit,
		sm.BaseQty = #tmp_po.BaseQty
	FROM Inventory.StockMutation sm, #tmp_po
	WHERE sm.RefCode1 = #tmp_po.Code
	AND sm.RefDetailId1 = #tmp_po.Id
	AND sm.Src = 'PO'

	-- Insert stock mutation that doesn't have with purchase order item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty,
			Code, Id,
			NULL, 'OI',
			'PO'
		FROM #tmp_po po
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = po.Code
			AND sm.RefDetailId1 = po.Id
			AND sm.Src = 'PO'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int	

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnIndent += @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
		ELSE
		BEGIN
			INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
			VALUES (@WHId,@ItemId,0,@Qty,0,0,0,GETDATE())
		END
		DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
	END
		
	-- Drop temp tables
	DROP TABLE #tmp_po
	DROP TABLE #tmp_wq

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_po') IS NOT NULL
		DROP TABLE #tmp_po 
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// Create procedure dbo.sp_update_stock_mutation_from_so
			sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_so]
	@code varchar(17),
	@date date
AS
BEGIN TRY
	DECLARE @warehouseCode varchar(8)

	SELECT @warehouseCode = WarehouseCode
	FROM Sales.SalesOrderHeader
	WHERE Code = @code

	SELECT so_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN so_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN so_d.Qty
			ELSE so_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_so
	FROM Sales.SalesOrderDetail so_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = so_d.UomId
		AND uom_c.Id = so_d.UnitId
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in sales order item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SO'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_so
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales order item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_so.ItemId,
		sm.UomId = #tmp_so.UomId,
		sm.UnitId = #tmp_so.UnitId,
		sm.Qty = #tmp_so.Qty,
		sm.BaseUnit = #tmp_so.BaseUnit,
		sm.BaseQty = #tmp_so.BaseQty
	FROM Inventory.StockMutation sm, #tmp_so
	WHERE sm.RefCode1 = #tmp_so.Code
	AND sm.RefDetailId1 = #tmp_so.Id
	AND sm.Src = 'SO'

	-- Insert stock mutation that doesn't have with sales order item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty,
			Code, Id,
			NULL, 'OO',
			'SO'
		FROM #tmp_so so
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = so.Code
			AND sm.RefDetailId1 = so.Id
			AND sm.Src = 'SO'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int	

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnOrder += @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
		ELSE
		BEGIN
			INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
			VALUES (@WHId,@ItemId,0,0,@Qty,0,0,GETDATE())
		END
		DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
	END
		
	-- Drop temp tables
	DROP TABLE #tmp_so
	DROP TABLE #tmp_wq

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_so') IS NOT NULL
		DROP TABLE #tmp_so
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// alter procedure dbo.sp_update_stock_mutation_from_do
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_do]
	@code varchar(17),
	@date date,
	@transCode varchar(17)
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
		sm.RefCode2 = @transCode
	FROM Inventory.StockMutation sm, #tmp_do
	WHERE sm.RefCode1 = #tmp_do.Code
	AND sm.RefDetailId1 = #tmp_do.Id
	AND sm.Src = 'DO'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id, @transCode,'OH', 'DO'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'DO'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, QtyOnOrder -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
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

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// alter procedure dbo.sp_update_stock_mutation_from_pr
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_pr]
	@code varchar(17),
	@date date,
	@rcvCode varchar(17)
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
	FROM Purchasing.PurchaseReturnDetail pr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = pr_d.UomId
		AND uom_c.Id = pr_d.UnitId
	WHERE Code = @code

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
	INTO #tmp_ex
	FROM Purchasing.PurchaseReturnDetailExchDiffItem pr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = pr_d.UomId
		AND uom_c.Id = pr_d.UnitId
	WHERE Code = @code
	
	-- Delete stock mutation that doesn't have in purchase receive item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PR'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_pr
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Delete stock mutation that doesn't have in purchase receive exchange item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PRX'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_ex
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
		sm.RefCode2 = CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'PR'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, 0, Code, Id,
			CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END, 'OH',
			'PR'
		FROM #tmp_pr pr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = pr.Code
			AND sm.RefDetailId1 = pr.Id
			AND sm.Src = 'PR'
		)

		-- Update stock mutation that exists in purchase receive exchange item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_ex.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_ex.ItemId,
		sm.UomId = #tmp_ex.UomId,
		sm.UnitId = #tmp_ex.UnitId,
		sm.Qty = #tmp_ex.Qty,
		sm.BaseUnit = #tmp_ex.BaseUnit,
		sm.BaseQty = #tmp_ex.BaseQty,
		sm.RefCode2 = CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_ex
	WHERE sm.RefCode1 = #tmp_ex.Code
	AND sm.RefDetailId1 = #tmp_ex.Id
	AND sm.Src = 'PRX'

	-- Insert stock mutation that doesn't have with purchase receive exchange item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, 0, Code, Id,
			CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END, 'OH',
			'PRX'
		FROM #tmp_ex pr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = pr.Code
			AND sm.RefDetailId1 = pr.Id
			AND sm.Src = 'PRX'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty, Src
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @Src varchar(max)

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @Src = Src FROM #tmp_wq


		IF (@Src = 'PR')
			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
				VALUES (@WHId,@ItemId,-@Qty,0,0,0,0,GETDATE())
			END
		ELSE
			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
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
	DROP TABLE #tmp_ex
	DROP TABLE #tmp_wq

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ex') IS NOT NULL
		DROP TABLE #tmp_ex

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// alter procedure dbo.sp_update_stock_mutation_from_rcv
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_rcv]
	@code varchar(17),
	@date date,
	@transCode varchar(17)
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
		sm.RefCode2 = CASE WHEN @transCode IS NOT NULL THEN @transCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'RCV'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty,
			Code, Id,
			CASE WHEN @transCode IS NOT NULL THEN @transCode ELSE NULL END, 'OH',
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
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, QtyOnIndent -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
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

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// alter procedure dbo.sp_update_stock_mutation_from_sr
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_sr]
	@code varchar(17),
	@date date,
	@dlvCode varchar(17),
	@warehouseCode varchar(20)
AS
BEGIN TRY

	SELECT sr_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.Qty
			ELSE sr_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_sr
	FROM Sales.SalesReturnDetail sr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = sr_d.UomId
		AND uom_c.Id = sr_d.UnitId
	WHERE Code = @code

	SELECT sr_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.Qty
			ELSE sr_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_ex
	FROM Sales.SalesReturnDetailExchDiffItem sr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = sr_d.UomId
		AND uom_c.Id = sr_d.UnitId
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in sales receive item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SR'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_sr
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Delete stock mutation that doesn't have in sales receive exchange item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SRX'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_ex
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales receive item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_sr.ItemId,
		sm.UomId = #tmp_sr.UomId,
		sm.UnitId = #tmp_sr.UnitId,
		sm.Qty = #tmp_sr.Qty,
		sm.BaseUnit = #tmp_sr.BaseUnit,
		sm.BaseQty = #tmp_sr.BaseQty,
		sm.RefCode2 = CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_sr
	WHERE sm.RefCode1 = #tmp_sr.Code
	AND sm.RefDetailId1 = #tmp_sr.Id
	AND sm.Src = 'SR'

	-- Insert stock mutation that doesn't have with sales receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, 0, Code, Id,
			CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END, 'OH',
			'SR'
		FROM #tmp_sr sr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = sr.Code
			AND sm.RefDetailId1 = sr.Id
			AND sm.Src = 'SR'
		)

		-- Update stock mutation that exists in sales receive exchange item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_ex.ItemId,
		sm.UomId = #tmp_ex.UomId,
		sm.UnitId = #tmp_ex.UnitId,
		sm.Qty = #tmp_ex.Qty,
		sm.BaseUnit = #tmp_ex.BaseUnit,
		sm.BaseQty = #tmp_ex.BaseQty,
		sm.RefCode2 = CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_ex
	WHERE sm.RefCode1 = #tmp_ex.Code
	AND sm.RefDetailId1 = #tmp_ex.Id
	AND sm.Src = 'SRX'

	-- Insert stock mutation that doesn't have with sales receive exchange item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, 0, Code, Id,
			CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END, 'OH',
			'SRX'
		FROM #tmp_ex sr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = sr.Code
			AND sm.RefDetailId1 = sr.Id
			AND sm.Src = 'SRX'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty, Src
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @Src varchar(max)

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @Src = Src FROM #tmp_wq


		IF (@Src = 'SR')
			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
				VALUES (@WHId,@ItemId,@Qty,0,0,0,0,GETDATE())
			END
		ELSE
			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
				VALUES (@WHId,@ItemId,-@Qty,0,0,0,0,GETDATE())
			END
	

		DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
	END

	-- Drop temp tables
	DROP TABLE #tmp_rtn
	DROP TABLE #tmp_ex
	DROP TABLE #tmp_wq

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_sr') IS NOT NULL
		DROP TABLE #tmp_sr
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ex') IS NOT NULL
		DROP TABLE #tmp_ex

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
			migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesmanGroup",
                schema: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_StockMutation_RefCode1",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropIndex(
                name: "IX_StockMutation_RefCode2",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropIndex(
                name: "IX_StockMutation_RefDetailId1",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropIndex(
                name: "IX_StockMutation_Src",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropIndex(
                name: "IX_StockMutation_Type",
                schema: "Inventory",
                table: "StockMutation");

            migrationBuilder.DropIndex(
                name: "IX_SalesDeliveryHeader_TransCode",
                schema: "Sales",
                table: "SalesDeliveryHeader");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReceiveHeader_TransCode",
                schema: "Purchasing",
                table: "PurchaseReceiveHeader");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryPlanDetail_TransCode",
                schema: "Sales",
                table: "DeliveryPlanDetail");

            migrationBuilder.DropIndex(
                name: "IX_DebitMemo_TransCode",
                schema: "Purchasing",
                table: "DebitMemo");

            migrationBuilder.DropIndex(
                name: "IX_CreditMemo_TransCode",
                schema: "Sales",
                table: "CreditMemo");
        }
    }
}
