using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterColumnAdjustment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "QtyAdjust",
                schema: "Inventory",
                table: "AdjustmentDetail",
                type: "decimal(19,6)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

			// Alter procedure dbo.sp_update_sr_dlv_qty
			var sql = @"ALTER PROCEDURE [dbo].[sp_update_sr_dlv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @returnQty decimal(18, 2), @dlvQty decimal(18, 2), @type int
	
	SELECT @type = [Type] FROM Sales.SalesReturnHeade rt_h WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
	IF (@type = 2)
	BEGIN
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
	END
	ELSE IF (@type = 3)
	BEGIN
		-- Get sales return data
		SELECT Code, ItemId, UnitId, Qty
		INTO #tmp_rtx
		FROM Sales.SalesReturnDetail rt_d
		WHERE EXISTS (
			SELECT Code
			FROM Sales.SalesReturnHeader rt_h
			WHERE Code = @code
			AND Mark NOT IN ('V', 'CLS')
			AND rt_h.Code = rt_d.Code
		)
	
		-- Return if sales return not exists
		IF NOT EXISTS (SELECT Code FROM #tmp_rtx)
		BEGIN
			DROP TABLE #tmp_rtx
			RETURN
		END
	
		-- Get sales delivery data
		SELECT ItemId, UnitId, SUM(Qty) AS QtyDlv
		INTO #tmp_dox
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
		SELECT rtx.Code, rtx.ItemId, rtx.UnitId, rtx.Qty,
			ISNULL(dox.QtyDlv, 0) AS QtyDlv
		INTO #tmp_allx
		FROM #tmp_rtx rtx
		LEFT JOIN #tmp_dox dox
			ON dox.ItemId = rtx.ItemId
			AND dox.UnitId = rtx.UnitId

		-- Drop temp tables
		DROP TABLE #tmp_rtx
		DROP TABLE #tmp_dox

		-- Calculate sum
		SELECT @returnQty = SUM(Qty),
			@dlvQty = SUM(QtyDlv)
		FROM #tmp_allx

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
		SET rt_d.QtyDlv = #tmp_allx.QtyDlv
		FROM Sales.SalesReturnDetail rt_d, #tmp_allx
		WHERE rt_d.Code = #tmp_allx.Code
		AND rt_d.ItemId = #tmp_allx.ItemId
		AND rt_d.UnitId = #tmp_allx.UnitId

		-- Drop temp table
		DROP TABLE #tmp_allx
	END

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

            // Alter procedure dbo.sp_update_pr_rcv_qty
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_adj]
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
				INSERT INTO Inventory.WarehouseQuantity (WarehouseCode, ItemId, QtyOnHand, QtyOnOrder, QtyOnIndent, QtyReorderPoint, QtyOnTransfer, UpdatedDate)
				VALUES (@WarehouseCode, @ItemId,@BaseQty,0,0,0,0,GetDate())
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

            // Refresh view dbo.sp_update_pr_rcv_qty
            sql = @"EXECUTE sp_refreshview 'Inventory.vwAdjustmentDetail'";
            migrationBuilder.Sql(sql);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "QtyAdjust",
                schema: "Inventory",
                table: "AdjustmentDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,6)");
        }
    }
}
