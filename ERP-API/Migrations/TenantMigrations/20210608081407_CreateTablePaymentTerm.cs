using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateTablePaymentTerm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentTerm",
                schema: "General",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Initial = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Due = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTerm", x => x.Id);
                });

            // Create view General.vwPaymentTerm
            var sql = @"CREATE VIEW [General].[vwPaymentTerm]
AS
	SELECT pt.*,
		u.Initial AS UpdatedInitial
	FROM General.PaymentTerm pt
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = pt.UpdatedBy";
            migrationBuilder.Sql(sql);

			// Create procedure dbo.sp_update_pr_rcv_qty
			sql = @"CREATE PROCEDURE [dbo].[sp_update_pr_rcv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @rtnQty decimal(18, 2), @rcvQty decimal(18, 2)

	-- Get purchase return data
	SELECT Code, ItemId, UnitId, Qty
	INTO #tmp_rt
	FROM Purchasing.PurchaseReturnDetail rt_d
	WHERE EXISTS (
		SELECT Code
		FROM Purchasing.PurchaseReturnHeader rt_h
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
		AND rt_h.Code = rt_d.Code
	)
	
	-- Return if purchase return not exists
	IF NOT EXISTS (SELECT Code FROM #tmp_rt)
	BEGIN
		DROP TABLE #tmp_rt
		RETURN
	END
	
	-- Get purchase receive data
	SELECT ItemId, UnitId, SUM(Qty) AS QtyRcv
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail pr_d
	WHERE EXISTS (
		SELECT Code
		FROM Purchasing.PurchaseReceiveHeader pr_h
		WHERE TransCode = @code
		AND Mark <> 'V'
		AND pr_h.Code = pr_d.Code
	)
	AND [Type] = 0
	GROUP BY ItemId, UnitId

	-- Join all
	SELECT rt.Code, rt.ItemId, rt.UnitId, rt.Qty,
		ISNULL(pr.QtyRcv, 0) AS QtyRcv
	INTO #tmp_all
	FROM #tmp_rt rt
	LEFT JOIN #tmp_pr pr
		ON pr.ItemId = rt.ItemId
		AND pr.UnitId = rt.UnitId

	-- Drop temp tables
	DROP TABLE #tmp_rt
	DROP TABLE #tmp_pr

	-- Calculate sum
	SELECT @rtnQty = SUM(Qty),
		@rcvQty = SUM(QtyRcv)
	FROM #tmp_all

	-- Update rt header mark
	UPDATE Purchasing.PurchaseReturnHeader
	SET Mark = (
		CASE WHEN @rcvQty = 0 THEN 'A'
			WHEN @rcvQty >= @rtnQty THEN 'CMP'
			ELSE 'PR' END
	)
	WHERE Code = @code
	AND Mark NOT IN ('V', 'CLS')

	-- Update rt detail receive qty item
	UPDATE rt_d
	SET rt_d.QtyRcv = #tmp_all.QtyRcv
	FROM Purchasing.PurchaseReturnDetail rt_d, #tmp_all
	WHERE rt_d.Code = #tmp_all.Code
	AND rt_d.ItemId = #tmp_all.ItemId
	AND rt_d.UnitId = #tmp_all.UnitId

	-- Drop temp table
	DROP TABLE #tmp_all

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_rt') IS NOT NULL
		DROP TABLE #tmp_rt
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// Create procedure dbo.sp_update_sr_dlv_qty
			sql = @"CREATE PROCEDURE [dbo].[sp_update_sr_dlv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @returnQty decimal(18, 2), @dlvQty decimal(18, 2)

	-- Get sales return data
	SELECT Code, ItemId, UnitId, Qty
	INTO #tmp_rt
	FROM Sales.SalesReturnDetail rt_d
	WHERE EXISTS (
		SELECT Code
		FROM Sales.SalesReturnHeader rt_h
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
		AND rt_h.Code = rt_d.Code
	)
	
	-- Return if sales return not exists
	IF NOT EXISTS (SELECT Code FROM #tmp_rt)
	BEGIN
		DROP TABLE #tmp_rt
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
	SELECT rt.Code, rt.ItemId, rt.UnitId, rt.Qty,
		ISNULL(do.QtyDlv, 0) AS QtyDlv
	INTO #tmp_all
	FROM #tmp_rt rt
	LEFT JOIN #tmp_do do
		ON do.ItemId = rt.ItemId
		AND do.UnitId = rt.UnitId

	-- Drop temp tables
	DROP TABLE #tmp_rt
	DROP TABLE #tmp_do

	-- Calculate sum
	SELECT @returnQty = SUM(Qty),
		@dlvQty = SUM(QtyDlv)
	FROM #tmp_all

	-- Update rt header mark
	UPDATE Sales.SalesReturnHeader
	SET Mark = (
		CASE WHEN @dlvQty = 0 THEN 'A'
			WHEN @dlvQty >= @returnQty THEN 'CMP'
			ELSE 'PS' END
	)
	WHERE Code = @code
	AND Mark NOT IN ('V', 'CLS')

	-- Update rt detail delivery qty item
	UPDATE rt_d
	SET rt_d.QtyDlv = #tmp_all.QtyDlv
	FROM Sales.SalesReturnDetail rt_d, #tmp_all
	WHERE rt_d.Code = #tmp_all.Code
	AND rt_d.ItemId = #tmp_all.ItemId
	AND rt_d.UnitId = #tmp_all.UnitId

	-- Drop temp table
	DROP TABLE #tmp_all

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_rt') IS NOT NULL
		DROP TABLE #tmp_rt
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

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
				'PR'
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
				'PR'
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
	DECLARE @OldQty decimal
	DECLARE @OldItemId int
	DECLARE @OldWhId varchar(max)
	DECLARE @srcTrans int

	SELECT @srcTrans = SrcTrans FROM Sales.SalesDeliveryHeader

	IF EXISTS(SELECT *FROM #tmp_ori_sm)
	BEGIN
		WHILE EXISTS(SELECT *FROM #tmp_ori_sm)
		BEGIN
			SELECT TOP 1 @OldQty = BaseQty, @OldItemId = ItemId, @OldWhId = WarehouseCode FROM #tmp_ori_sm

			IF (@srcTrans = 1)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @OldItemId) AND ItemId = @OldItemId
			END
			ELSE
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
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
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
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
			UPDATE Inventory.WarehouseQuantity SET QtyOnOrder -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = (SELECT WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId) AND ItemId = @ItemId
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
				'SR'
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
				'SR'
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentTerm",
                schema: "General");
        }
    }
}
