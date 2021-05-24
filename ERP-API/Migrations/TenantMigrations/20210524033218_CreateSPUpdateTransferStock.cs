using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateSPUpdateTransferStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create procedure dbo.sp_update_transfer_stock
            var sql = @"CREATE PROCEDURE [dbo].[sp_update_transfer_stock]
	@code varchar(17),
	@date date,
	@stockType smallint,
	@whCodeFrom varchar(8),
	@whCodeTo varchar(8),
	@IsComplete bit
AS
BEGIN TRY

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

	-- Update WarehouseQty
	DECLARE @itemID AS INT = 0
	DECLARE @qty AS DECIMAL = 0

	IF (@IsComplete = 0)
	BEGIN
		IF (@stockType = 1) --InventoryOut
		BEGIN
			WHILE EXISTS(SELECT * FROM #tmp_its)
			BEGIN
				SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

				IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnTransfer = @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
					VALUES (@whCodeFrom,@ItemId,@qty,0,0,0,@qty,GETDATE())
				END

				DELETE #tmp_its WHERE ItemId = @ItemId
			END
		END
		ELSE IF (@stockType = 3) --DirectTransfer
		BEGIN
			WHILE EXISTS(SELECT * FROM #tmp_its)
			BEGIN
				SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

				IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

					IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
					END
					ELSE
					BEGIN
						INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
						VALUES (@whCodeTo,@ItemId,@qty,0,0,0,0,GETDATE())
					END
				END
				ELSE
				BEGIN
					IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
					END
					ELSE
					BEGIN
						INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
						VALUES (@whCodeTo,@ItemId,@qty,0,0,0,0,GETDATE())
					END
				END

				DELETE #tmp_its WHERE ItemId = @ItemId
			END
		END
	END
	ELSE
	BEGIN
		WHILE EXISTS(SELECT * FROM #tmp_its)
		BEGIN
			SELECT TOP 1 @itemID = ItemId, @qty = BaseQty FROM #tmp_its

			IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeFrom AND ItemId = @itemID)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, QtyOnTransfer = QtyOnTransfer - @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

				IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
					VALUES (@whCodeTo,@ItemId,@qty,0,0,0,0,GETDATE())
				END
			END
			ELSE
			BEGIN
				IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
				END
				ELSE
				BEGIN
					INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
					VALUES (@whCodeTo,@ItemId,@qty,0,0,0,0,GETDATE())
				END
			END

			DELETE #tmp_its WHERE ItemId = @ItemId
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

			// Alter procedure dbo.sp_update_so_dlv_qty
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
		WHERE TransCode = @code
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, Code, BaseUnit, BaseQty, Id, @transCode, 'DO'
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
		sm.RefCode2 = CASE WHEN [Type] = 0 THEN @transCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'RCV'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty,
			Code, Id,
			CASE WHEN [Type] = 0 THEN @transCode ELSE NULL END,
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

			// Alter view Inventory.vwTransferStockHeader
			sql = @"ALTER VIEW [Inventory].[vwTransferStockHeader]
AS
    SELECT ts_h.*,
        w_f.Initial AS WarehouseInitialFrom,
        w_t.Initial AS WarehouseInitialTo,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        CASE ts_h.Mark
            WHEN 'A' THEN 'Active'
            WHEN 'V' THEN 'Void' END AS [Status],
        CASE ts_h.[Type]
            WHEN 1 THEN 'Barang Keluar'
            WHEN 2 THEN 'Barang Masuk'
            WHEN 3 THEN 'Transfer Langsung' END AS TypeInitial
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop procedure dbo.sp_update_transfer_stock
            var sql = @"DROP PROCEDURE [dbo].[sp_update_transfer_stock]";
            migrationBuilder.Sql(sql);
		}
	}
}
