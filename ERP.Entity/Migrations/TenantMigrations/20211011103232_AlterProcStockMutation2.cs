using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterProcStockMutation2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Alter procedure dbo.sp_update_stock_mutation_from_po
			var sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_po]
	@code varchar(17),
	@date date,
	@isVoid bit = 0
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

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PO'

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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0,
			Code, Id, NULL, 'OI', 'PO'
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
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm

			UPDATE Inventory.WarehouseQuantity SET QtyOnIndent -= @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	IF(@isVoid = 0)
	BEGIN
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
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END
	
	-- Drop temp tables
	DROP TABLE #tmp_po
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_po') IS NOT NULL
		DROP TABLE #tmp_po 
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_update_stock_mutation_from_do
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_do]
	@code varchar(17),
	@date date,
	@transCode varchar(17),
	@isVoid bit = 0
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
		sm.RefCode2 = @transCode
	FROM Inventory.StockMutation sm, #tmp_do
	WHERE sm.RefCode1 = #tmp_do.Code
	AND sm.RefDetailId1 = #tmp_do.Id
	AND sm.Src = 'DO'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id, @transCode,'OH', 'DO'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'DO'
		)
	
	-- Free Goods
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
	INTO #tmp_do_free
	FROM Sales.SalesDeliveryDetailFreeGood do_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = do_d.UomId
		AND uom_c.Id = do_d.UnitId
	WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm_free
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'DOF'

	-- Delete stock mutation that doesn't have in sales delivery free item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'DOF'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_do_free
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales delivery free item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_do_free.ItemId,
		sm.UomId = #tmp_do_free.UomId,
		sm.UnitId = #tmp_do_free.UnitId,
		sm.Qty = #tmp_do_free.Qty,
		sm.BaseUnit = #tmp_do_free.BaseUnit,
		sm.BaseQty = #tmp_do_free.BaseQty,
		sm.RefCode2 = @transCode
	FROM Inventory.StockMutation sm, #tmp_do_free
	WHERE sm.RefCode1 = #tmp_do_free.Code
	AND sm.RefDetailId1 = #tmp_do_free.Id
	AND sm.Src = 'DOF'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id, @transCode,'OH', 'DOF'
		FROM #tmp_do_free do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'DOF'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)
	DECLARE @srcTrans int

	SELECT @srcTrans = SrcTrans FROM Sales.SalesDeliveryHeader WHERE Code = @code

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm

			IF (@srcTrans = 1)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId) AND ItemId = @OldItemId
			END
			ELSE
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
			END
			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	IF EXISTS(SELECT *FROM #tmp_ori_sm_free)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm_free)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm_free

			IF (@srcTrans = 1)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId) AND ItemId = @OldItemId
			END
			ELSE
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
			END
			DELETE #tmp_ori_sm_free WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				IF (@srcTrans = 1)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
				VALUES (@WHId,@ItemId,-@Qty,0,-@Qty,0,0,GETDATE())
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END

	-- Drop temp tables
	DROP TABLE #tmp_do
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm
	DROP TABLE #tmp_do_free
	DROP TABLE #tmp_ori_sm_free

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm
	IF OBJECT_ID('tempdb.dbo.#tmp_do_free') IS NOT NULL
		DROP TABLE #tmp_do_free
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm_free') IS NOT NULL
		DROP TABLE #tmp_ori_sm_free


	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_update_stock_mutation_from_rcv
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_rcv]
	@code varchar(17),
	@date date,
	@transCode varchar(17),
	@isVoid bit = 0
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
			) END AS BaseQty,
		CASE WHEN pr_h.IncludeTax = 1 THEN 
			(pr_d.NettPrice - (CASE WHEN pr_h.FinalDisc > 0 THEN pr_h.FinalDisc * pr_d.NettPrice / (SELECT SUM(NettPrice) FROM Purchasing.PurchaseReceiveDetail WHERE Code = @code) ELSE 0 END)) - pr_d.TaxAmount
			ELSE pr_d.NettPrice - (CASE WHEN pr_h.FinalDisc > 0 THEN pr_h.FinalDisc * pr_d.NettPrice / (SELECT SUM(NettPrice) FROM Purchasing.PurchaseReceiveDetail WHERE Code = @code) ELSE 0 END)
			END AS FinalNettPrice,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN
			CASE WHEN pr_h.IncludeTax = 1 THEN 
			(pr_d.NettPrice - (CASE WHEN pr_h.FinalDisc > 0 THEN pr_h.FinalDisc * pr_d.NettPrice / (SELECT SUM(NettPrice) FROM Purchasing.PurchaseReceiveDetail WHERE Code = @code) ELSE 0 END)) - pr_d.TaxAmount
			ELSE pr_d.NettPrice - (CASE WHEN pr_h.FinalDisc > 0 THEN pr_h.FinalDisc * pr_d.NettPrice / (SELECT SUM(NettPrice) FROM Purchasing.PurchaseReceiveDetail WHERE Code = @code) ELSE 0 END)
			END 
		ELSE
			CASE WHEN pr_h.IncludeTax = 1 THEN 
			((pr_d.NettPrice - (CASE WHEN pr_h.FinalDisc > 0 THEN pr_h.FinalDisc * pr_d.NettPrice / (SELECT SUM(NettPrice) FROM Purchasing.PurchaseReceiveDetail WHERE Code = @code) ELSE 0 END)) - pr_d.TaxAmount) / 
			(
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			)
			ELSE (pr_d.NettPrice - (CASE WHEN pr_h.FinalDisc > 0 THEN pr_h.FinalDisc * pr_d.NettPrice / (SELECT SUM(NettPrice) FROM Purchasing.PurchaseReceiveDetail WHERE Code = @code) ELSE 0 END)) / 
			(
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			)
			END
		END AS BaseNettPrice
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail pr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = pr_d.UomId
		AND uom_c.Id = pr_d.UnitId
	LEFT JOIN Purchasing.PurchaseReceiveHeader pr_h
		ON pr_h.Code = pr_d.Code
	WHERE pr_d.Code = @code

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
		sm.RefCode2 = CASE WHEN @transCode IS NOT NULL THEN @transCode ELSE NULL END,
		sm.BaseNettPrice = #tmp_pr.BaseNettPrice,
		sm.NettPrice = #tmp_pr.FinalNettPrice
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'RCV'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, FinalNettPrice, BaseUnit, BaseQty, BaseNettPrice,
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
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)
	DECLARE @srcTrans int

	SELECT @srcTrans = SrcTrans FROM Purchasing.PurchaseReceiveHeader

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm
			
			IF (@srcTrans = 1)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnIndent += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId) AND ItemId = @OldItemId
			END
			ELSE
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
			END
			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				IF (@srcTrans = 1)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					UPDATE Inventory.WarehouseQuantity SET QtyOnIndent -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
				VALUES (@WHId,@ItemId,@Qty,0,0,0,0,GETDATE())
				UPDATE Inventory.WarehouseQuantity SET QtyOnIndent -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
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

			// Alter procedure dbo.sp_update_stock_mutation_from_so
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_so]
	@code varchar(17),
	@date date,
	@isVoid bit = 0
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

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SO'

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
	SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0,
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

	-- Free Goods
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
	INTO #tmp_so_free
	FROM Sales.SalesOrderDetailFreeGood so_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = so_d.UomId
		AND uom_c.Id = so_d.UnitId
	WHERE Code = @code 

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm_free 
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SOF'

	-- Delete stock mutation that doesn't have in sales order free good item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SOF'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_so_free 
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in sales order free good item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_so_free.ItemId,
		sm.UomId = #tmp_so_free.UomId,
		sm.UnitId = #tmp_so_free.UnitId,
		sm.Qty = #tmp_so_free.Qty,
		sm.BaseUnit = #tmp_so_free.BaseUnit,
		sm.BaseQty = #tmp_so_free.BaseQty
	FROM Inventory.StockMutation sm, #tmp_so_free 
	WHERE sm.RefCode1 = #tmp_so_free.Code
	AND sm.RefDetailId1 = #tmp_so_free.Id
	AND sm.Src = 'SOF'

	-- Insert stock mutation that doesn't have with sales order free good item detail
	INSERT INTO Inventory.StockMutation
	SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0,
		Code, Id,
		NULL, 'OO',
		'SOF'
	FROM #tmp_so_free so
	WHERE NOT EXISTS (
		SELECT Id
		FROM Inventory.StockMutation sm
		WHERE sm.RefCode1 = so.Code
		AND sm.RefDetailId1 = so.Id
		AND sm.Src = 'SOF'
	)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int 
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm

			UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	IF EXISTS(SELECT *FROM #tmp_ori_sm_free)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm_free)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm_free

			UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm_free WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END 

	IF(@isVoid = 0)
	BEGIN
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
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
	END

	-- Drop temp tables
	DROP TABLE #tmp_so
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm
	DROP TABLE #tmp_so_free
	DROP TABLE #tmp_ori_sm_free

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_so') IS NOT NULL
		DROP TABLE #tmp_so
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm
	IF OBJECT_ID('tempdb.dbo.#tmp_so_free') IS NOT NULL
		DROP TABLE #tmp_so_free
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm_free') IS NOT NULL
		DROP TABLE #tmp_ori_sm_free

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
