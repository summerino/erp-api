using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class AlterProcTransferStock : Migration
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
	DECLARE @stockType smallint
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
	AND Src = 'TS'

    -- Update WarehouseQty
    DECLARE @itemID AS INT = 0
    DECLARE @qty AS DECIMAL = 0

    IF (@IsComplete = 0) --Active
    BEGIN

		IF (@stockType != 3) --InventoryIn & Out
		BEGIN
			DECLARE @loop INT = 0
			WHILE (@loop < 2)
			BEGIN
				-- Loop 0 = OH, Loop 1 = OT
				-- Insert stock mutation transfer stock item detail OH & OT
				INSERT INTO Inventory.StockMutation
					SELECT CASE WHEN @stockType = 1 THEN @whCodeFrom 
								WHEN @stockType = 2 AND @loop = 0 THEN @whCodeTo 
								WHEN @stockType = 2 AND @loop = 1 THEN @whCodeFrom END, 
						@date, ItemId, UomId, UnitId, 
						CASE WHEN @stockType = 1 AND @loop = 0 THEN Qty * -1 
							 WHEN @stockType = 1 AND @loop = 1 THEN Qty
							 WHEN @stockType = 2 AND @loop = 0 THEN Qty 
							 WHEN @stockType = 2 AND @loop = 1 THEN Qty * -1 END, 
						BaseUnit, 
						CASE WHEN @stockType = 1 AND @loop = 0 THEN BaseQty * -1 
							 WHEN @stockType = 1 AND @loop = 1 THEN BaseQty
							 WHEN @stockType = 2 AND @loop = 0 THEN BaseQty 
							 WHEN @stockType = 2 AND @loop = 1 THEN BaseQty * -1 END,
						Code, Id,
						CASE WHEN @stockType = 1 THEN NULL WHEN @stockType = 2 THEN @originTransferCode END, 
						CASE WHEN @stockType = 1 AND @loop = 0 THEN 'OH' 
							 WHEN @stockType = 1 AND @loop = 1 THEN 'OT'
							 WHEN @stockType = 2 AND @loop = 0 THEN 'OH' 
							 WHEN @stockType = 2 AND @loop = 1 THEN 'OT' END,
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

        IF (@stockType = 1) --InventoryOut
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
        ELSE IF (@stockType = 2) --InventoryIn
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
        ELSE IF (@stockType = 3) --DirectTransfer
        BEGIN
			-- Insert stock mutation transfer stock item detail warehouse from
			INSERT INTO Inventory.StockMutation
				SELECT @whCodeFrom, @date, ItemId, UomId, UnitId, Qty * -1, BaseUnit, BaseQty * -1,
					Code, Id,
					NULL, 'OH',
					'TS'
				FROM #tmp_its

			-- Insert stock mutation transfer stock item detail warehouse to
			INSERT INTO Inventory.StockMutation
				SELECT @whCodeTo, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty,
					Code, Id,
					NULL, 'OH',
					'TS'
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
        IF (@stockType = 1)
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
        ELSE IF (@stockType = 2)
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
        ELSE IF (@stockType = 3)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
