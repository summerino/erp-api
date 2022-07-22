using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterProcStockMutation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter procedure dbo.sp_update_stock_mutation_from_adj
            var sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_adj]
	@code varchar(17),
	@date date
AS
SET NOCOUNT ON
BEGIN TRY

	SELECT d.*, h.WarehouseCode
	INTO #tmp_adj
	FROM Inventory.AdjustmentDetail d
	JOIN Inventory.AdjustmentHeader h on d.code = h.code
	WHERE d.Code = @code 

	-- Restore data warehouse qty from stock mutation
	SELECT *
	INTO #tmp_old_stk
	FROM Inventory.StockMutation 
	where RefCode1 = @code

	DECLARE @Id int
	DECLARE @UomId int
	DECLARE @UnitId int
	DECLARE @BaseQty int
	DECLARE @ItemId int
	DECLARE @WarehouseCode varchar(100)

	WHILE EXISTS(SELECT * FROM #tmp_old_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_old_stk
		SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
		UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand - @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
		DELETE #tmp_old_stk WHERE Id = @Id
	END
	drop table #tmp_old_stk

	-- Delete stock mutation that doesn't have in adjustment item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'ADJ'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_adj
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in adjustment item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_adj.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_adj.ItemId,
		sm.UomId = #tmp_adj.UomId,
		sm.UnitId = #tmp_adj.UnitId,
		sm.Qty = #tmp_adj.QtyAdjust,
		sm.RefCode2 = NULL
	FROM Inventory.StockMutation sm, #tmp_adj
	WHERE sm.RefCode1 = #tmp_adj.Code
	AND sm.RefDetailId1 = #tmp_adj.Id
	AND sm.Src = 'ADJ'

	-- Insert stock mutation that doesn't have with adjustment item detail
	INSERT INTO Inventory.StockMutation
		SELECT 
			WarehouseCode as WarehouseCode, 
			@date as [Date], 
			ItemId as ItemId, 
			UomId as UomId, 
			UnitId as UnitId, 
			QtyAdjust as Qty,
			0 as NettPrice,
			UnitId as BaseUnit, --test
			0 as BaseQty,
			0 as BaseNettPrice,
			Code as RefCode1, 
			Id as RefDetailId,
			NULL as RefCode,
			'OH' as [Type],
			'ADJ' as Src
		FROM #tmp_adj adj
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = adj.Code
			AND sm.RefDetailId1 = adj.Id
			AND sm.Src = 'ADJ'
		) And adj.QtyAdjust <> 0
	-- Drop temp tables
	DROP TABLE #tmp_adj
	
	DECLARE @IsBaseUnit int
	DECLARE @BaseUnitQty int
	DECLARE @Seq int

	SELECT *
	INTO #tmp_stk
	FROM Inventory.StockMutation WHERE RefCode1 = @code
	
	WHILE EXISTS(SELECT * FROM #tmp_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_stk
		SELECT @IsBaseUnit = IsBaseUnit, @Seq = Seq FROM Inventory.UoMConversion WHERE UomId = @UomId AND Id = @UnitId

		IF @IsBaseUnit = 1 
		BEGIN
			UPDATE Inventory.StockMutation SET BaseQty = Qty,BaseUnit = UnitId WHERE Id = @Id			
		END
		ELSE
		BEGIN
			UPDATE Inventory.StockMutation 
			SET BaseQty = ROUND(Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq), 0),
			BaseUnit = (SELECT Id FROM Inventory.UoMConversion WHERE UomId = @UomId AND IsBaseUnit = 1) WHERE Id = @Id
		END
		
		-- update new warehouse quantity
		IF EXISTS (SELECT ID FROM Inventory.WarehouseQuantity WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode)
			BEGIN
				SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
				UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand + @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
			END
		ELSE 
		BEGIN
			BEGIN
				SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
				UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand + @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
				INSERT INTO Inventory.WarehouseQuantity (WarehouseCode, ItemId, QtyOnHand, QtyOnOrder, QtyOnIndent, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
				VALUES (@WarehouseCode, @ItemId, @BaseQty, 0, 0, 0, 0, 0, dbo.udf_current_local_time())
			END
		END
		
		DELETE #tmp_stk WHERE Id = @Id
	END

	DROP TABLE #tmp_stk
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_adj') IS NOT NULL
		DROP TABLE #tmp_adj
	IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
		DROP TABLE #tmp_old_stk
	IF OBJECT_ID('tempdb.dbo.#tmp_stk') IS NOT NULL
		DROP TABLE #tmp_stk
	
	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Alter procedure dbo.sp_update_stock_mutation_from_bb
            sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_bb]
	@code varchar(17),
	@date date
AS
SET NOCOUNT ON
BEGIN TRY

	SELECT d.*, h.WarehouseCode
	INTO #tmp_bb
	FROM Inventory.BeginningBalanceDetail d
	JOIN Inventory.BeginningBalanceHeader h on d.code = h.code
	WHERE d.Code = @code 

	-- Restore data warehouse qty from stock mutation
	SELECT *
	INTO #tmp_old_stk
	FROM Inventory.StockMutation 
	where RefCode1 = @code

	DECLARE @Id int
	DECLARE @UomId int
	DECLARE @UnitId int
	DECLARE @BaseQty int
	DECLARE @ItemId int
	DECLARE @WarehouseCode varchar(100)

	WHILE EXISTS(SELECT * FROM #tmp_old_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_old_stk
		SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
		UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand - @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
		DELETE #tmp_old_stk WHERE Id = @Id
	END
	drop table #tmp_old_stk

	-- Delete stock mutation that doesn't have in bb item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'BB'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_bb
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in bb item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_bb.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_bb.ItemId,
		sm.UomId = #tmp_bb.UomId,
		sm.UnitId = #tmp_bb.UnitId,
		sm.Qty = #tmp_bb.Qty,
		sm.NettPrice = #tmp_bb.UnitPrice,
		sm.RefCode2 = NULL
	FROM Inventory.StockMutation sm, #tmp_bb
	WHERE sm.RefCode1 = #tmp_bb.Code
	AND sm.RefDetailId1 = #tmp_bb.Id
	AND sm.Src = 'BB'

	-- Insert stock mutation that doesn't have with bb item detail
	INSERT INTO Inventory.StockMutation
		SELECT 
			WarehouseCode as WarehouseCode, 
			@date as [Date], 
			ItemId as ItemId, 
			UomId as UomId, 
			UnitId as UnitId, 
			Qty as Qty,
			UnitPrice as NettPrice,
			UnitId as BaseUnit, 
			0 as BaseQty,
			0 as BaseNettPrice,
			Code as RefCode1, 
			Id as RefDetailId,
			NULL as RefCode,
			'OH' as [Type],
			'BB' as Src
		FROM #tmp_bb bb
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = bb.Code
			AND sm.RefDetailId1 = bb.Id
			AND sm.Src = 'BB'
		) And bb.Qty <> 0
	-- Drop temp tables
	DROP TABLE #tmp_bb
	
	DECLARE @IsBaseUnit int
	DECLARE @BaseUnitQty int
	DECLARE @Seq int

	SELECT *
	INTO #tmp_stk
	FROM Inventory.StockMutation WHERE RefCode1 = @code
	
	WHILE EXISTS(SELECT * FROM #tmp_stk)
	BEGIN
		SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_stk
		SELECT @IsBaseUnit = IsBaseUnit, @Seq = Seq FROM Inventory.UoMConversion WHERE UomId = @UomId AND Id = @UnitId

		IF @IsBaseUnit = 1 
		BEGIN
			UPDATE Inventory.StockMutation SET BaseQty = Qty,BaseUnit = UnitId,BaseNettPrice = NettPrice WHERE Id = @Id			
		END
		ELSE
		BEGIN
			UPDATE Inventory.StockMutation 
			SET BaseQty = Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
			BaseNettPrice = NettPrice / (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
			BaseUnit = (SELECT Id FROM Inventory.UoMConversion WHERE UomId = @UomId AND IsBaseUnit = 1) WHERE Id = @Id
		END
		
		-- update new warehouse quantity
		IF EXISTS (SELECT ID FROM Inventory.WarehouseQuantity WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode)
			BEGIN
				SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
				UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand + @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
			END
		ELSE 
		BEGIN
			BEGIN
				SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
				UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand + @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
				INSERT INTO Inventory.WarehouseQuantity (WarehouseCode, ItemId, QtyOnHand, QtyOnOrder, QtyOnIndent, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
				VALUES (@WarehouseCode, @ItemId, @BaseQty, 0, 0, 0, 0, 0, dbo.udf_current_local_time())
			END
		END
		
		DELETE #tmp_stk WHERE Id = @Id
	END

	DROP TABLE #tmp_stk
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_bb') IS NOT NULL
		DROP TABLE #tmp_bb
	IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
		DROP TABLE #tmp_old_stk
	IF OBJECT_ID('tempdb.dbo.#tmp_stk') IS NOT NULL
		DROP TABLE #tmp_stk
	
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, -Qty, 0, BaseUnit, -BaseQty, 0, Code, Id, @transCode,'OH', 'DO'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.[Type] = 'OH'
			AND sm.Src = 'DO'
		)
	
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, -Qty, 0, BaseUnit, -BaseQty, 0, Code, Id, @transCode,'OO', 'DO'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.[Type] = 'OO'
			AND sm.Src = 'DO'
		)

	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id, @transCode,'OTS', 'DO'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.[Type] = 'OTS'
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, -Qty, 0, BaseUnit, -BaseQty, 0, Code, Id, @transCode,'OH', 'DOF'
		FROM #tmp_do_free do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.[Type] = 'OH'
			AND sm.Src = 'DOF'
		)

	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, -Qty, 0, BaseUnit, -BaseQty, 0, Code, Id, @transCode,'OO', 'DOF'
		FROM #tmp_do_free do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.[Type] = 'OO'
			AND sm.Src = 'DOF'
		)

	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id, @transCode,'OTS', 'DOF'
		FROM #tmp_do_free do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.[Type] = 'OTS'
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
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId) AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnTransit -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId) AND ItemId = @OldItemId

			END
			ELSE
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
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
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId) AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnTransit -= @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId) AND ItemId = @OldItemId
			END
			ELSE
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
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
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					UPDATE Inventory.WarehouseQuantity SET QtyOnOrder += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
					UPDATE Inventory.WarehouseQuantity SET QtyOnTransit += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
				VALUES (@WHId, @ItemId, 0, 0, 0, 0, 0, 0, dbo.udf_current_local_time())
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnTransit += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
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
				IF (@srcTrans = 1)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
					UPDATE Inventory.WarehouseQuantity SET QtyOnTransit -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
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

            // Alter procedure dbo.sp_update_stock_mutation_from_po
            sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_po]
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
	SELECT WarehouseCode, ItemId, BaseQty, UnitId
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int	
	DECLARE @UnitId int

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnIndent += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
				VALUES (@WHId, @ItemId, 0, @Qty, 0, 0, 0, 0, dbo.udf_current_local_time())
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnIndent -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
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

            // Alter procedure dbo.sp_update_stock_mutation_from_pr
            sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_pr]
	@code varchar(17),
	@date date,
	@rcvCode varchar(17),
	@isVoid bit = 0
AS
BEGIN TRY

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PR'

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
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id,
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

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty, UnitId
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @UnitId int

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
				VALUES (@WHId, @ItemId, -@Qty, 0, 0, 0, 0, 0, dbo.udf_current_local_time())
			END

			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code AND UnitId = @UnitId
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END

	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
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
		pr_d.NettPrice - pr_d.TaxAmount + pr_d.ExemptTaxAmount
		AS FinalNettPrice,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN
			pr_d.NettPrice - pr_d.TaxAmount + pr_d.ExemptTaxAmount
		ELSE
			(pr_d.NettPrice - pr_d.TaxAmount + pr_d.ExemptTaxAmount) / 
			(
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			)
		END AS BaseNettPrice
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail pr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = pr_d.UomId
		AND uom_c.Id = pr_d.UnitId
	LEFT JOIN Purchasing.PurchaseReceiveHeader pr_h
		ON pr_h.Code = pr_d.Code
	WHERE pr_d.Code = @code

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
	SELECT WarehouseCode, ItemId, BaseQty, UnitId
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @srcTrans int
	DECLARE @UnitId int

	SELECT @srcTrans = SrcTrans FROM Purchasing.PurchaseReceiveHeader WHERE Code = @code

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				IF (@srcTrans = 1)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					UPDATE Inventory.WarehouseQuantity SET QtyOnIndent -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId AND UnitId = @UnitId) AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
				VALUES (@WHId, @ItemId, @Qty, 0, 0, 0, 0, 0, dbo.udf_current_local_time())
				UPDATE Inventory.WarehouseQuantity SET QtyOnIndent -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId AND UnitId = @UnitId) AND ItemId = @ItemId
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId AND RefCode1 = @code)
			BEGIN
				IF (@srcTrans = 1)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					UPDATE Inventory.WarehouseQuantity SET QtyOnIndent += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId AND UnitId = @UnitId) AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
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

			// Create procedure dbo.sp_update_stock_mutation_from_si
			sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_si]
	@code varchar(17),
	@date date,
	@transCode varchar(17),
	@isVoid bit = 0
AS
BEGIN TRY

	DECLARE @warehouseCode varchar(8)

	SELECT @warehouseCode = WarehouseCode
	FROM Sales.SalesDeliveryHeader
	WHERE Code = @transCode

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
	WHERE Code = @transCode

	-- Delete stock mutation that doesn't have in sales delivery item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SI'
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
	WHERE sm.RefCode1 = @code
	AND sm.RefDetailId1 = #tmp_do.Id
	AND sm.Src = 'SI'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, @code, Id, @transCode, 'OTS', 'SI'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode2 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'SI'
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
	WHERE Code = @transCode

	-- Delete stock mutation that doesn't have in sales delivery free item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SIF'
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
	WHERE sm.RefCode1 = @code
	AND sm.RefDetailId1 = #tmp_do_free.Id
	AND sm.Src = 'DOF'

	-- Insert stock mutation that doesn't have with sales delivery item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, @code, Id, @transCode,'OTS', 'SIF'
		FROM #tmp_do_free do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode2 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'DOF'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty, UnitId
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code AND Src IN ('SI', 'SIF')

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @srcTrans int
	DECLARE @UnitId int

	SELECT @srcTrans = SrcTrans FROM Sales.SalesDeliveryHeader WHERE Code = @transCode

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				IF (@srcTrans = 1)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnTransit -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode =  @WHId AND ItemId = @ItemId
				END
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
				VALUES (@WHId, @ItemId, 0, 0, 0, 0, 0, 0, dbo.udf_current_local_time())
				UPDATE Inventory.WarehouseQuantity SET QtyOnTransit -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				IF (@srcTrans = 1)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnTransit += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code AND UnitId = @UnitId
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END

	-- Drop temp tables
	DROP TABLE #tmp_do
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_do_free

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_do_free') IS NOT NULL
		DROP TABLE #tmp_do_free

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
	SELECT WarehouseCode, ItemId, BaseQty, UnitId
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code AND Src IN ('SO', 'SOF')

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int 
	DECLARE @UnitId int

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
				VALUES (@WHId, @ItemId, 0, 0, @Qty, 0, 0, dbo.udf_current_local_time())
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId

				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code AND UnitId = @UnitId
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END

	-- Drop temp tables
	DROP TABLE #tmp_so
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_so_free

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_so') IS NOT NULL
		DROP TABLE #tmp_so
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_so_free') IS NOT NULL
		DROP TABLE #tmp_so_free

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

            // Alter procedure dbo.sp_update_stock_mutation_from_sr
            sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_sr]
	@code varchar(17),
	@date date,
	@dlvCode varchar(17),
	@warehouseCode varchar(20),
	@isVoid bit = 0
AS
BEGIN TRY

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SR'

	SELECT sr_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.Qty
			ELSE sr_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.NettPrice
			ELSE sr_d.NettPrice / (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseNettPrice
	INTO #tmp_sr
	FROM Sales.SalesReturnDetail sr_d
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

	-- Update stock mutation that exists in sales receive item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_sr.ItemId,
		sm.UomId = #tmp_sr.UomId,
		sm.UnitId = #tmp_sr.UnitId,
		sm.Qty = #tmp_sr.Qty,
		sm.NettPrice = #tmp_sr.NettPrice,
		sm.BaseUnit = #tmp_sr.BaseUnit,
		sm.BaseQty = #tmp_sr.BaseQty,
		sm.BaseNettPrice = #tmp_sr.BaseNettPrice,
		sm.RefCode2 = CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_sr
	WHERE sm.RefCode1 = #tmp_sr.Code
	AND sm.RefDetailId1 = #tmp_sr.Id
	AND sm.Src = 'SR'

	-- Insert stock mutation that doesn't have with sales receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, NettPrice, BaseUnit, BaseQty, BaseNettPrice, Code, Id,
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
	

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty, UnitId
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @UnitId int

	IF(@isVoid = 0)
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
				VALUES (@WHId, @ItemId, @Qty, 0, 0, 0, 0, 0, dbo.udf_current_local_time())
			END

			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId

				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code AND UnitId = @UnitId
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId
		END
	END

	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_sr') IS NOT NULL
		DROP TABLE #tmp_sr
	DROP TABLE #tmp_wq

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_sr') IS NOT NULL
		DROP TABLE #tmp_sr
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_update_transfer_stock
			sql = @"ALTER PROCEDURE [dbo].[sp_update_transfer_stock]
    @code varchar(17),
    @date date,
    @IsComplete bit
AS
BEGIN TRY
	DECLARE @stockType varchar(8)
    DECLARE @whCodeFrom varchar(8)
    DECLARE @whCodeTo varchar(8)
	DECLARE @originTransferCode varchar(17)

	SELECT @stockType = [Type], @whCodeFrom = WarehouseCodeFrom, @whCodeTo = WarehouseCodeTo, @originTransferCode = OriginTransferCode
	FROM Inventory.TransferStockHeader
	WHERE Code = @code

    SELECT i_ts.*,
        CASE WHEN uom_c.IsBaseUnit = 1 THEN i_ts.UnitId
            ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
        CASE WHEN uom_c.IsBaseUnit = 1 THEN i_ts.Qty
            ELSE i_ts.Qty * (
                SELECT EXP(SUM(LOG(Conversion)))
                FROM Inventory.UoMConversion
                WHERE UomId = uom_c.UomId
                AND Seq <= uom_c.Seq
            ) END AS BaseQty
    INTO #tmp_its
    FROM [Inventory].[TransferStockDetail] i_ts
    LEFT JOIN Inventory.UoMConversion uom_c
        ON uom_c.UomId = i_ts.UomId
        AND uom_c.Id = i_ts.UnitId
    WHERE Code = @code

	-- Delete stock mutation transfer stock item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src IN ('TS','CNEE')

    -- Update WarehouseQty
    DECLARE @itemID AS INT = 0
    DECLARE @qty AS DECIMAL = 0

    IF (@IsComplete = 0) --Active
    BEGIN

		IF (@stockType NOT IN ('DT','C','RC')) --InventoryIn & Out
		BEGIN
			DECLARE @loop INT = 0
			WHILE (@loop < 2)
			BEGIN
				-- Loop 0 = OH, Loop 1 = OT
				-- Insert stock mutation transfer stock item detail OH & OT
				INSERT INTO Inventory.StockMutation
					SELECT CASE WHEN @stockType = 'OUT' THEN @whCodeFrom 
								WHEN @stockType = 'IN' AND @loop = 0 THEN @whCodeTo 
								WHEN @stockType = 'IN' AND @loop = 1 THEN @whCodeFrom END, 
						@date, ItemId, UomId, UnitId, 
						CASE WHEN @stockType = 'OUT' AND @loop = 0 THEN Qty * -1 
							 WHEN @stockType = 'OUT' AND @loop = 1 THEN Qty
							 WHEN @stockType = 'IN' AND @loop = 0 THEN Qty 
							 WHEN @stockType = 'IN' AND @loop = 1 THEN Qty * -1 END, 
						0/*NettPrice */, BaseUnit, 
						CASE WHEN @stockType = 'OUT' AND @loop = 0 THEN BaseQty * -1 
							 WHEN @stockType = 'OUT' AND @loop = 1 THEN BaseQty
							 WHEN @stockType = 'IN' AND @loop = 0 THEN BaseQty 
							 WHEN @stockType = 'IN' AND @loop = 1 THEN BaseQty * -1 END,
						0/*BaseNettPrice */,Code, Id,
						CASE WHEN @stockType = 'OUT' THEN NULL WHEN @stockType = 'IN' THEN @originTransferCode END, 
						CASE WHEN @stockType = 'OUT' AND @loop = 0 THEN 'OH' 
							 WHEN @stockType = 'OUT' AND @loop = 1 THEN 'OT'
							 WHEN @stockType = 'IN' AND @loop = 0 THEN 'OH' 
							 WHEN @stockType = 'IN' AND @loop = 1 THEN 'OT' END,
						'TS'
					FROM #tmp_its its
					--WHERE NOT EXISTS (
					--	SELECT Id
					--	FROM Inventory.StockMutation sm
					--	WHERE sm.RefCode1 = its.Code
					--	AND sm.RefDetailId1 = its.Id
					--	AND sm.Src = 'TS'
					--)

				SET @loop = @loop + 1
			END
		END

        IF (@stockType = 'OUT') --InventoryOut
        BEGIN
            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, QtyOnTransfer = QtyOnTransfer + @qty, UpdatedDate = dbo.udf_current_local_time() 
                    WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
        ELSE IF (@stockType = 'IN') --InventoryIn
        BEGIN
            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                UPDATE Inventory.WarehouseQuantity SET QtyOnTransfer = QtyOnTransfer - @qty, UpdatedDate = dbo.udf_current_local_time() 
                WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = dbo.udf_current_local_time() 
                    WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
                END
                ELSE
                BEGIN
                    INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
                    VALUES (@whCodeTo, @ItemId, @qty, 0, 0, 0, 0, 0, dbo.udf_current_local_time())
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
        ELSE IF (@stockType IN ('DT','C','RC')) --DirectTransfer & Consignee
        BEGIN
			-- Insert stock mutation transfer stock item detail warehouse from
			INSERT INTO Inventory.StockMutation
				SELECT @whCodeFrom, @date, ItemId, UomId, UnitId, Qty * -1, 0/*NettPrice */, BaseUnit, BaseQty * -1, 0/*BaseNettPrice */,
					Code, Id,
					NULL, 'OH',
					CASE WHEN @stockType = 'DT' THEN 'TS'
						 WHEN @stockType IN ('C','RC') THEN 'CNEE' END
				FROM #tmp_its

			-- Insert stock mutation transfer stock item detail warehouse to
			INSERT INTO Inventory.StockMutation
				SELECT @whCodeTo, @date, ItemId, UomId, UnitId, Qty, 0/*NettPrice */, BaseUnit, BaseQty, 0/*BaseNettPrice */,
					Code, Id,
					NULL, 'OH',
					CASE WHEN @stockType = 'DT' THEN 'TS'
						 WHEN @stockType IN ('C','RC') THEN 'CNEE' END
				FROM #tmp_its
				--WHERE NOT EXISTS (
				--	SELECT Id
				--	FROM Inventory.StockMutation sm
				--	WHERE sm.RefCode1 = its.Code
				--	AND sm.RefDetailId1 = its.Id
				--	AND sm.Src = 'TS'
				--)

            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                    IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                    BEGIN
                        UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
                        VALUES (@whCodeTo, @ItemId, @qty, 0, 0, 0, 0, 0, dbo.udf_current_local_time())
                    END
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
    END
    ELSE --Void = rollback item
    BEGIN
        IF (@stockType = 'OUT')
        BEGIN
            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, QtyOnTransfer = QtyOnTransfer - @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
        ELSE IF (@stockType = 'IN')
        BEGIN
            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnTransfer = QtyOnTransfer + @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                    IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                    BEGIN
                        UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
                    END
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
        ELSE IF (@stockType IN ('DT','C','RC'))
        BEGIN
            WHILE EXISTS(SELECT * FROM #tmp_its)
            BEGIN
                SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                    IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                    BEGIN
                        UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
                    END
                END

                DELETE #tmp_its WHERE ItemId = @ItemId
            END
        END
    END
        
    -- Drop temp tables
    DROP TABLE #tmp_its
 

END TRY
BEGIN CATCH
    -- Drop temp tables
    IF OBJECT_ID('tempdb.dbo.#tmp_its') IS NOT NULL
        DROP TABLE #tmp_its

    -- Raise error
    EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_refresh_wh_qty
			sql = @"ALTER PROCEDURE [dbo].[sp_refresh_wh_qty]   
	-- Add the parameters for the stored procedure here  
AS  
BEGIN TRY  
	-- SET NOCOUNT ON added to prevent extra result sets from  
	-- interfering with SELECT statements.  
	SET NOCOUNT ON;
	SET ANSI_WARNINGS OFF;

	-- Select Data  
	WITH cte_on_transfer AS (  
		SELECT sm.WarehouseCode, sm.ItemId, CAST(SUM(sm.BaseQty) AS decimal(19,8)) AS TotalBaseQty 
		From Inventory.StockMutation sm 
		LEFT JOIN Inventory.TransferStockHeader ts ON sm.RefCode1 = ts.Code
		WHERE sm.[Type] = 'OT' AND ts.[Type] = 'OUT' AND ts.Mark = 'A'
		GROUP BY sm.WarehouseCode, sm.ItemId    
	),  
	cte_on_hand AS (  
		SELECT WarehouseCode, ItemId, CAST(SUM(BaseQtyValue) AS decimal(19,8)) AS TotalBaseQty FROM 
		( 
			SELECT *, 
			CASE
				WHEN Src IN ('BB','RCV','SR') THEN BaseQty
				WHEN Src IN ('DO', 'DOF', 'PR') THEN -BaseQty
				ELSE BaseQty
			END BaseQtyValue
			FROM Inventory.StockMutation
		) sm
		WHERE [Type] = 'OH' GROUP BY WarehouseCode, ItemId   
	),
	cte_base_qty_order_free AS (
		SELECT so_d.Id,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN so_d.Qty
				ELSE so_d.Qty * (
					SELECT EXP(SUM(LOG(Conversion)))
					FROM Inventory.UoMConversion
					WHERE UomId = uom_c.UomId
					AND Seq <= uom_c.Seq
				) END AS BaseQty,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN so_d.QtyClosed
				ELSE so_d.QtyClosed * (
					SELECT EXP(SUM(LOG(Conversion)))
					FROM Inventory.UoMConversion
					WHERE UomId = uom_c.UomId
					AND Seq <= uom_c.Seq
				) END AS BaseQtyDlv
		FROM Sales.SalesOrderDetailFreeGood so_d
		LEFT JOIN Sales.SalesOrderHeader so_h
			ON so_h.Code = so_d.Code
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.UomId = so_d.UomId
			AND uom_c.Id = so_d.UnitId
		WHERE so_h.Mark NOT IN ('V', 'OL', 'CLS', 'CMP')
	),
	cte_base_qty_order AS (
		SELECT so_d.Id,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN so_d.Qty
				ELSE so_d.Qty * (
					SELECT EXP(SUM(LOG(Conversion)))
					FROM Inventory.UoMConversion
					WHERE UomId = uom_c.UomId
					AND Seq <= uom_c.Seq
				) END AS BaseQty,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN so_d.QtyDlv
				ELSE so_d.QtyDlv * (
					SELECT EXP(SUM(LOG(Conversion)))
					FROM Inventory.UoMConversion
					WHERE UomId = uom_c.UomId
					AND Seq <= uom_c.Seq
				) END AS BaseQtyDlv
		FROM Sales.SalesOrderDetail so_d
		LEFT JOIN Sales.SalesOrderHeader so_h
			ON so_h.Code = so_d.Code
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.UomId = so_d.UomId
			AND uom_c.Id = so_d.UnitId
		WHERE so_h.Mark NOT IN ('V', 'OL', 'CLS', 'CMP')
	),
	cte_on_order AS (  
		SELECT sm.WarehouseCode, sm.ItemId, 
		ISNULL(CAST(
			CASE WHEN SUM(so_d.BaseQtyDlv - so_d.BaseQty) < 0 THEN ABS(SUM(so_d.BaseQtyDlv - so_d.BaseQty))
			ELSE SUM(so_d.BaseQtyDlv - so_d.BaseQty)
			END
		AS decimal(19,8)), CAST(0 as decimal(19,8))) AS TotalBaseQty,
		ISNULL(CAST(
			CASE WHEN SUM(so_df.BaseQtyDlv - so_df.BaseQty) < 0 THEN ABS(SUM(so_df.BaseQtyDlv - so_df.BaseQty))
			ELSE SUM(so_df.BaseQtyDlv - so_df.BaseQty)
			END
		AS decimal(19,8)), CAST(0 as decimal(19,8))) AS TotalBaseQtyFree
		FROM (
			SELECT *
			FROM Inventory.StockMutation
			WHERE [Type] = 'OO'
		) sm
		LEFT JOIN cte_base_qty_order so_d
			ON so_d.Id = sm.RefDetailId1 AND sm.Src = 'SO'
		LEFT JOIN cte_base_qty_order_free so_df
			ON so_df.Id = sm.RefDetailId1 AND sm.Src = 'SOF'
		GROUP BY sm.WarehouseCode, sm.ItemId
	),
	cte_base_qty_indent AS (
		SELECT po_d.Id,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN po_d.Qty
				ELSE po_d.Qty * (
					SELECT EXP(SUM(LOG(Conversion)))
					FROM Inventory.UoMConversion
					WHERE UomId = uom_c.UomId
					AND Seq <= uom_c.Seq
				) END AS BaseQty,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN po_d.QtyRcv
				ELSE po_d.QtyRcv * (
					SELECT EXP(SUM(LOG(Conversion)))
					FROM Inventory.UoMConversion
					WHERE UomId = uom_c.UomId
					AND Seq <= uom_c.Seq
				) END AS BaseQtyRcv
		FROM Purchasing.PurchaseOrderDetail po_d
		LEFT JOIN Purchasing.PurchaseOrderHeader po_h
			ON po_h.Code = po_d.Code
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.UomId = po_d.UomId
			AND uom_c.Id = po_d.UnitId
		WHERE po_h.Mark NOT IN ('V', 'CLS', 'CMP')
	),
	cte_on_indent AS (  
		SELECT sm.WarehouseCode, sm.ItemId,
		CAST(
			CASE WHEN SUM(po_d.BaseQtyRcv - po_d.BaseQty) < 0 THEN ABS(SUM(po_d.BaseQtyRcv - po_d.BaseQty))
			ELSE SUM(po_d.BaseQtyRcv - po_d.BaseQty)
			END
		AS decimal(19,8)) AS TotalBaseQty
		FROM (
			SELECT *
			FROM Inventory.StockMutation
			WHERE [Type] = 'OI'
		) sm
		LEFT JOIN cte_base_qty_indent po_d
		ON po_d.Id = sm.RefDetailId1
		GROUP BY sm.WarehouseCode, sm.ItemId
	),
	cte_base_qty_transit AS (
		SELECT do_d.Id,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN do_d.Qty
				ELSE do_d.Qty * (
					SELECT EXP(SUM(LOG(Conversion)))
					FROM Inventory.UoMConversion
					WHERE UomId = uom_c.UomId
					AND Seq <= uom_c.Seq
				) END AS BaseQty
		FROM Sales.SalesDeliveryDetail do_d
		LEFT JOIN Sales.SalesDeliveryHeader do_h
			ON do_h.Code = do_d.Code
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.UomId = do_d.UomId
			AND uom_c.Id = do_d.UnitId
		WHERE do_h.Mark = 'A'
	),
	cte_on_transit AS (  
		SELECT sm.WarehouseCode, sm.ItemId, 
		ISNULL(SUM(do_d.BaseQty), CAST(0 as decimal(19,8))) AS TotalBaseQty
		FROM (
			SELECT *
			FROM Inventory.StockMutation
			WHERE [Type] = 'OTS'
		) sm
		LEFT JOIN cte_base_qty_transit do_d
			ON do_d.Id = sm.RefDetailId1 AND sm.Src = 'DO'
		GROUP BY sm.WarehouseCode, sm.ItemId
	)  

	SELECT dt.WarehouseCode, dt.ItemId,   
		CASE  
			WHEN SUM(oh.TotalBaseQty) IS NOT NULL THEN SUM(oh.TotalBaseQty)  
			ELSE CAST(0 AS decimal(19,8))  
		END AS QtyOnHand,   
		CASE  
			WHEN SUM(oi.TotalBaseQty) IS NOT NULL THEN SUM(oi.TotalBaseQty)  
			ELSE CAST(0 AS decimal(19,8))  
		END AS QtyOnIndent,  
		CASE  
			WHEN SUM(oo.TotalBaseQty + oo.TotalBaseQtyFree) IS NOT NULL THEN SUM(oo.TotalBaseQty + oo.TotalBaseQtyFree)
			ELSE CAST(0 AS decimal(19,8))  
		END AS QtyOnOrder,  
		CASE  
			WHEN SUM(ot.TotalBaseQty) IS NOT NULL THEN SUM(ot.TotalBaseQty)  
			ELSE CAST(0 AS decimal(19,8))  
		END AS QtyOnTransfer,
		CASE  
			WHEN SUM(ots.TotalBaseQty) IS NOT NULL THEN SUM(ots.TotalBaseQty)  
			ELSE CAST(0 AS decimal(19,8))  
		END AS QtyOnTransit, 
		CAST(0 AS decimal(19,8)) AS QtyReorderPoint  
	INTO #tmp_dt  
	FROM Inventory.WarehouseQuantity dt  
	LEFT JOIN cte_on_hand oh ON dt.WarehouseCode = oh.WarehouseCode AND dt.ItemId = oh.ItemId  
	LEFT JOIN cte_on_indent oi ON dt.WarehouseCode = oi.WarehouseCode AND dt.ItemId = oi.ItemId  
	LEFT JOIN cte_on_order oo ON dt.WarehouseCode = oo.WarehouseCode AND dt.ItemId = oo.ItemId  
	LEFT JOIN cte_on_transfer ot ON dt.WarehouseCode = ot.WarehouseCode AND dt.ItemId = ot.ItemId 
	LEFT JOIN cte_on_transit ots ON dt.WarehouseCode = ots.WarehouseCode AND dt.ItemId = ots.ItemId 
	GROUP BY dt.WarehouseCode, dt.ItemId  
	ORDER BY dt.WarehouseCode, dt.ItemId  


	-- Update Process  
	DECLARE @WHId varchar(max)  
	DECLARE @ItemId int  
	DECLARE @QtyOO decimal(19,8)  
	DECLARE @QtyOH decimal(19,8)  
	DECLARE @QtyOI decimal(19,8)  
	DECLARE @QtyOT decimal(19,8)  
	DECLARE @QtyRP decimal(19,8)  
	DECLARE @QtyOTS decimal(19,8) 

	IF EXISTS(SELECT *FROM #tmp_dt)  
	BEGIN  
		WHILE EXISTS(SELECT *FROM #tmp_dt)  
		BEGIN  
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @QtyOH = QtyOnHand, @QtyOI = QtyOnIndent, @QtyOO = QtyOnOrder, @QtyOT = QtyOnTransfer, @QtyRP = QtyReorderPoint, @QtyOTS = QtyOnTransit FROM #tmp_dt  
			--Update Data  
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand = @QtyOH, QtyOnIndent = @QtyOI, QtyOnOrder = @QtyOO, QtyOnTransfer = @QtyOT, QtyReorderPoint = @QtyRP, QtyOnTransit = @QtyOTS, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId  
			DELETE #tmp_dt WHERE WarehouseCode = @WHId AND ItemId = @ItemId  
		END
	END  

	PRINT('Proses Selesai')  
END TRY  
BEGIN CATCH  
	-- Drop temp tables  
	IF OBJECT_ID('tempdb.dbo.#tmp_dt') IS NOT NULL  
	DROP TABLE #tmp_dt  

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
