using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterProcTransferStock2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter procedure dbo.sp_update_transfer_stock
            var sql = @"ALTER PROCEDURE [dbo].[sp_update_transfer_stock]
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
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, QtyOnTransfer = QtyOnTransfer + @qty, UpdatedDate = GETDATE() 
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

                UPDATE Inventory.WarehouseQuantity SET QtyOnTransfer = QtyOnTransfer - @qty, UpdatedDate = GETDATE() 
                WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                BEGIN
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = GETDATE() 
                    WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
                END
                ELSE
                BEGIN
                    INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
                    VALUES (@whCodeTo,@ItemId,@qty,0,0,0,0,GETDATE())
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
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, QtyOnTransfer = QtyOnTransfer - @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId
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
                    UPDATE Inventory.WarehouseQuantity SET QtyOnTransfer = QtyOnTransfer + @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                    IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                    BEGIN
                        UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
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
                    UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeFrom AND ItemId = @ItemId

                    IF EXISTS(SELECT * FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @whCodeTo AND ItemId = @itemID)
                    BEGIN
                        UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @whCodeTo AND ItemId = @ItemId
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
            WHEN 'IN' THEN 'Barang Keluar'
            WHEN 'OUT'THEN 'Barang Masuk'
            WHEN 'DT' THEN 'Transfer Langsung'
			WHEN 'C' THEN 'Titip Barang'
			WHEN 'RC' THEN 'Retur Titipan'END AS TypeInitial
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
        }
    }
}
