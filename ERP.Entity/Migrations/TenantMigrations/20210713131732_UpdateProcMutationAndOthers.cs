using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class UpdateProcMutationAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create view Finance.vwInterCashBankHeader
            var sql = @"CREATE VIEW Finance.vwInterCashBankHeader
AS
    SELECT gcbh.*,
        gcbd.TransCode,
        gcbd.CoaCode AS CoaDetail,
        gcbd.CurrCode AS CurrDetail,
        gcbd.Rate AS RateDetail,
        gcbd.Amount AS AmountDetail,
        gcbd.[Type] AS TypeDetail,
        gcbd.TypeAmount,
        gcbd.TransAmount,
        gcbd.Notes AS NotesDetail,
        c_f.[Name] AS CoaNameFrom,
        c_t.[Name] AS CoaNameTo,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE gcbh.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void'
            WHEN 'CMP' THEN 'Completed'
            WHEN 'CLS' THEN 'Closed' END AS [Status]
    FROM Finance.GeneralCashBankHeader gcbh
    LEFT JOIN Finance.GeneralCashBankDetail gcbd
        ON gcbh.Code = gcbd.Code and gcbd.[Type] = 'ICBO'
    LEFT JOIN Accounting.COA c_f
        ON c_f.Code = gcbh.CoaCode
    LEFT JOIN Accounting.COA c_t
        ON c_t.Code = gcbd.CoaCode
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = gcbh.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = gcbh.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = gcbh.ApprovedBy
    WHERE gcbd.TransCode IS NOT NULL";
			migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_update_stock_mutation_from_so
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_so]
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
	SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty,
		Code, Id,
		NULL, 'OO',
		'SO', 0, 0
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
	SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty,
		Code, Id,
		NULL, 'OO',
		'SOF', 0, 0
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

			// Alter procedure dbo.sp_update_stock_mutation_from_sr
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_sr]
	@code varchar(17),
	@date date,
	@dlvCode varchar(17),
	@warehouseCode varchar(20)
AS
BEGIN TRY

	DECLARE @Type int
	SELECT @Type = [Type] FROM Sales.SalesReturnHeader WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SR'

	IF (@Type = 2)
	BEGIN
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
			sm.BaseUnit = #tmp_sr.BaseUnit,
			sm.BaseQty = #tmp_sr.BaseQty,
			sm.RefCode2 = CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END
		FROM Inventory.StockMutation sm, #tmp_sr
		WHERE sm.RefCode1 = #tmp_sr.Code
		AND sm.RefDetailId1 = #tmp_sr.Id
		AND sm.Src = 'SR'

		-- Insert stock mutation that doesn't have with sales receive item detail
		INSERT INTO Inventory.StockMutation
			SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id,
				CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END, 'OH',
				'SR', 0, 0
			FROM #tmp_sr sr
			WHERE NOT EXISTS (
				SELECT Id
				FROM Inventory.StockMutation sm
				WHERE sm.RefCode1 = sr.Code
				AND sm.RefDetailId1 = sr.Id
				AND sm.Src = 'SR'
			)
	END
	ELSE IF (@Type = 3)
	BEGIN
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
		INTO #tmp_srx
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
			FROM #tmp_srx
			WHERE Code = RefCode1
			AND Id = RefDetailId1
		)

		-- Update stock mutation that exists in sales receive item detail
		UPDATE sm
		SET sm.WarehouseCode = @warehouseCode,
			sm.[Date] = @date,
			sm.ItemId = #tmp_srx.ItemId,
			sm.UomId = #tmp_srx.UomId,
			sm.UnitId = #tmp_srx.UnitId,
			sm.Qty = #tmp_srx.Qty,
			sm.BaseUnit = #tmp_srx.BaseUnit,
			sm.BaseQty = #tmp_srx.BaseQty,
			sm.RefCode2 = CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END
		FROM Inventory.StockMutation sm, #tmp_srx
		WHERE sm.RefCode1 = #tmp_srx.Code
		AND sm.RefDetailId1 = #tmp_srx.Id
		AND sm.Src = 'SR'

		-- Insert stock mutation that doesn't have with sales receive item detail
		INSERT INTO Inventory.StockMutation
			SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id,
				CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END, 'OH',
				'SR', 0, 0
			FROM #tmp_srx sr
			WHERE NOT EXISTS (
				SELECT Id
				FROM Inventory.StockMutation sm
				WHERE sm.RefCode1 = sr.Code
				AND sm.RefDetailId1 = sr.Id
				AND sm.Src = 'SR'
			)
	END
	

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

			UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

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
	IF OBJECT_ID('tempdb.dbo.#tmp_sr') IS NOT NULL
		DROP TABLE #tmp_sr
	IF OBJECT_ID('tempdb.dbo.#tmp_srx') IS NOT NULL
		DROP TABLE #tmp_srx
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_sr') IS NOT NULL
		DROP TABLE #tmp_sr
	IF OBJECT_ID('tempdb.dbo.#tmp_srx') IS NOT NULL
		DROP TABLE #tmp_srx
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id, @transCode,'OH', 'DO', 0, 0
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id, @transCode,'OH', 'DOF', 0, 0
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty,
			Code, Id, NULL, 'OI', 'PO', 0, 0
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

			// Alter procedure dbo.sp_update_stock_mutation_from_pr
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_pr]
	@code varchar(17),
	@date date,
	@rcvCode varchar(17)
AS
BEGIN TRY

	DECLARE @Type int
	SELECT @Type = [Type] FROM Purchasing.PurchaseReturnHeader WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PR'

	IF (@Type = 2)
	BEGIN
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
			SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id,
				CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END, 'OH',
				'PR', 0, 0
			FROM #tmp_pr pr
			WHERE NOT EXISTS (
				SELECT Id
				FROM Inventory.StockMutation sm
				WHERE sm.RefCode1 = pr.Code
				AND sm.RefDetailId1 = pr.Id
				AND sm.Src = 'PR'
			)
	END
	ELSE IF (@Type = 3)
	BEGIN
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
		INTO #tmp_prx
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
			FROM #tmp_prx
			WHERE Code = RefCode1
			AND Id = RefDetailId1
		)

		-- Update stock mutation that exists in purchase receive item detail
		UPDATE sm
		SET sm.WarehouseCode = #tmp_prx.WarehouseCode,
			sm.[Date] = @date,
			sm.ItemId = #tmp_prx.ItemId,
			sm.UomId = #tmp_prx.UomId,
			sm.UnitId = #tmp_prx.UnitId,
			sm.Qty = #tmp_prx.Qty,
			sm.BaseUnit = #tmp_prx.BaseUnit,
			sm.BaseQty = #tmp_prx.BaseQty,
			sm.RefCode2 = CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END
		FROM Inventory.StockMutation sm, #tmp_prx
		WHERE sm.RefCode1 = #tmp_prx.Code
		AND sm.RefDetailId1 = #tmp_prx.Id
		AND sm.Src = 'PR'

		-- Insert stock mutation that doesn't have with purchase receive item detail
		INSERT INTO Inventory.StockMutation
			SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id,
				CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END, 'OH',
				'PR', 0, 0
			FROM #tmp_prx pr
			WHERE NOT EXISTS (
				SELECT Id
				FROM Inventory.StockMutation sm
				WHERE sm.RefCode1 = pr.Code
				AND sm.RefDetailId1 = pr.Id
				AND sm.Src = 'PR'
			)
	END

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
			
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

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
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_prx') IS NOT NULL
		DROP TABLE #tmp_prx
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_prx') IS NOT NULL
		DROP TABLE #tmp_prx
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
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty,
			Code, Id,
			CASE WHEN @transCode IS NOT NULL THEN @transCode ELSE NULL END, 'OH',
			'RCV', BaseNettPrice, FinalNettPrice
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

            // Reorder column SystemManagement.User
            sql = @"BEGIN TRANSACTION
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
ALTER TABLE SystemManagement.[User]
	DROP CONSTRAINT FK_User_Role_RoleId
GO
ALTER TABLE SystemManagement.Role SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE SystemManagement.[User]
	DROP CONSTRAINT FK_User_Employee_EmployeeId
GO
ALTER TABLE General.Employee SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE SystemManagement.Tmp_User
	(
	Id int NOT NULL IDENTITY (1, 1),
	CatalogUserId uniqueidentifier NOT NULL,
	Username nvarchar(50) NOT NULL,
	Initial varchar(20) NOT NULL,
	Name varchar(50) NOT NULL,
	RoleId int NOT NULL,
	EmployeeId bigint NULL,
	MobileSignIn bit NOT NULL,
	IsLoggedIn bit NOT NULL,
	LastLogin datetime NULL,
	SessionId varchar(50) NULL,
	TokenId varchar(MAX) NULL,
	IPAddress varchar(40) NULL,
	IsMobileLoggedIn bit NOT NULL,
	MobileLastLogin datetime NULL,
	MobileSessionId varchar(50) NULL,
	MobileTokenId varchar(MAX) NULL,
	MobileIPAddress varchar(40) NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE SystemManagement.Tmp_User SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT SystemManagement.Tmp_User ON
GO
IF EXISTS(SELECT * FROM SystemManagement.[User])
	 EXEC('INSERT INTO SystemManagement.Tmp_User (Id, CatalogUserId, Username, Initial, Name, RoleId, EmployeeId, MobileSignIn, IsLoggedIn, LastLogin, SessionId, TokenId, IPAddress, IsMobileLoggedIn, MobileLastLogin, MobileSessionId, MobileTokenId, MobileIPAddress, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, CatalogUserId, Username, Initial, Name, RoleId, EmployeeId, MobileSignIn, IsLoggedIn, LastLogin, SessionId, TokenId, IPAddress, IsMobileLoggedIn, MobileLastLogin, MobileSessionId, MobileTokenId, MobileIPAddress, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM SystemManagement.[User] WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT SystemManagement.Tmp_User OFF
GO
DROP TABLE SystemManagement.[User]
GO
EXECUTE sp_rename N'SystemManagement.Tmp_User', N'User', 'OBJECT' 
GO
ALTER TABLE SystemManagement.[User] ADD CONSTRAINT
	PK_User PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_User_EmployeeId ON SystemManagement.[User]
	(
	EmployeeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_User_RoleId ON SystemManagement.[User]
	(
	RoleId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE SystemManagement.[User] ADD CONSTRAINT
	FK_User_Employee_EmployeeId FOREIGN KEY
	(
	EmployeeId
	) REFERENCES General.Employee
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE SystemManagement.[User] ADD CONSTRAINT
	FK_User_Role_RoleId FOREIGN KEY
	(
	RoleId
	) REFERENCES SystemManagement.Role
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Refresh view
            sql = @"execute sp_refreshview 'SystemManagement.vwUser'";
            migrationBuilder.Sql(sql);

            // Reorder column Inventory.StockMutation
            sql = @"BEGIN TRANSACTION
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
ALTER TABLE Inventory.StockMutation
	DROP CONSTRAINT FK_StockMutation_UoMConversion_UnitId
GO
ALTER TABLE Inventory.StockMutation
	DROP CONSTRAINT FK_StockMutation_UoMConversion_BaseUnit
GO
ALTER TABLE Inventory.UoMConversion SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockMutation
	DROP CONSTRAINT FK_StockMutation_UoM_UomId
GO
ALTER TABLE Inventory.UoM SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockMutation
	DROP CONSTRAINT FK_StockMutation_Warehouse_WarehouseCode
GO
ALTER TABLE Inventory.Warehouse SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.StockMutation
	DROP CONSTRAINT FK_StockMutation_Item_ItemId
GO
ALTER TABLE Inventory.Item SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Tmp_StockMutation
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	WarehouseCode varchar(8) NOT NULL,
	Date date NOT NULL,
	ItemId int NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Qty decimal(18, 6) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	BaseUnit int NOT NULL,
	BaseQty decimal(18, 6) NOT NULL,
	BaseNettPrice decimal(19, 6) NOT NULL,
	RefCode1 varchar(17) NOT NULL,
	RefDetailId1 bigint NOT NULL,
	RefCode2 varchar(17) NULL,
	Type varchar(5) NOT NULL,
	Src varchar(10) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_StockMutation SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT Inventory.Tmp_StockMutation ON
GO
IF EXISTS(SELECT * FROM Inventory.StockMutation)
	 EXEC('INSERT INTO Inventory.Tmp_StockMutation (Id, WarehouseCode, Date, ItemId, UomId, UnitId, Qty, NettPrice, BaseUnit, BaseQty, BaseNettPrice, RefCode1, RefDetailId1, RefCode2, Type, Src)
		SELECT Id, WarehouseCode, Date, ItemId, UomId, UnitId, Qty, NettPrice, BaseUnit, BaseQty, BaseNettPrice, RefCode1, RefDetailId1, RefCode2, Type, Src FROM Inventory.StockMutation WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT Inventory.Tmp_StockMutation OFF
GO
DROP TABLE Inventory.StockMutation
GO
EXECUTE sp_rename N'Inventory.Tmp_StockMutation', N'StockMutation', 'OBJECT' 
GO
ALTER TABLE Inventory.StockMutation ADD CONSTRAINT
	PK_StockMutation PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_StockMutation_RefCode1 ON Inventory.StockMutation
	(
	RefCode1
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_StockMutation_RefCode2 ON Inventory.StockMutation
	(
	RefCode2
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_StockMutation_RefDetailId1 ON Inventory.StockMutation
	(
	RefDetailId1
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_StockMutation_Src ON Inventory.StockMutation
	(
	Src
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_StockMutation_Type ON Inventory.StockMutation
	(
	Type
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_StockMutation_UnitId ON Inventory.StockMutation
	(
	UnitId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_StockMutation_UomId ON Inventory.StockMutation
	(
	UomId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_StockMutation_WarehouseCode ON Inventory.StockMutation
	(
	WarehouseCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_StockMutation_ItemId ON Inventory.StockMutation
	(
	ItemId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_StockMutation_BaseUnit ON Inventory.StockMutation
	(
	BaseUnit
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Inventory.StockMutation ADD CONSTRAINT
	FK_StockMutation_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockMutation ADD CONSTRAINT
	FK_StockMutation_Warehouse_WarehouseCode FOREIGN KEY
	(
	WarehouseCode
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockMutation ADD CONSTRAINT
	FK_StockMutation_UoM_UomId FOREIGN KEY
	(
	UomId
	) REFERENCES Inventory.UoM
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockMutation ADD CONSTRAINT
	FK_StockMutation_UoMConversion_UnitId FOREIGN KEY
	(
	UnitId
	) REFERENCES Inventory.UoMConversion
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.StockMutation ADD CONSTRAINT
	FK_StockMutation_UoMConversion_BaseUnit FOREIGN KEY
	(
	BaseUnit
	) REFERENCES Inventory.UoMConversion
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Alter foreign key in Finance.GeneralCashBankDetail
            sql = @"BEGIN TRANSACTION
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
ALTER TABLE Finance.GeneralCashBankDetail
    DROP CONSTRAINT FK_GeneralCashBankDetail_CashBankType_Type
GO
ALTER TABLE Finance.CashBankType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Finance.GeneralCashBankDetail WITH NOCHECK ADD CONSTRAINT
    FK_GeneralCashBankDetail_CashBankType_Type FOREIGN KEY
    (
    Type
    ) REFERENCES Finance.CashBankType
    (
    Code
    ) ON UPDATE  NO ACTION
     ON DELETE  NO ACTION
   
GO
ALTER TABLE Finance.GeneralCashBankDetail
    NOCHECK CONSTRAINT FK_GeneralCashBankDetail_CashBankType_Type
GO
ALTER TABLE Finance.GeneralCashBankDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop view Finance.vwInterCashBankHeader
            var sql = @"DROP VIEW Finance.vwInterCashBankHeader";
            migrationBuilder.Sql(sql);
        }
    }
}
