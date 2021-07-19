using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTablePromoSubject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromoSubject",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    CustCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    CustTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoSubject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoSubject_Customer_CustCode",
                        column: x => x.CustCode,
                        principalSchema: "General",
                        principalTable: "Customer",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_PromoSubject_CustomerType_CustTypeId",
                        column: x => x.CustTypeId,
                        principalSchema: "General",
                        principalTable: "CustomerType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PromoSubject_PromoHeader_Code",
                        column: x => x.Code,
                        principalSchema: "Sales",
                        principalTable: "PromoHeader",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromoDetailTier_PromoDetailId",
                schema: "Sales",
                table: "PromoDetailTier",
                column: "PromoDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoDetail_Code",
                schema: "Sales",
                table: "PromoDetail",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_PromoSubject_Code",
                schema: "Sales",
                table: "PromoSubject",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_PromoSubject_CustCode",
                schema: "Sales",
                table: "PromoSubject",
                column: "CustCode");

            migrationBuilder.CreateIndex(
                name: "IX_PromoSubject_CustTypeId",
                schema: "Sales",
                table: "PromoSubject",
                column: "CustTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoDetail_PromoHeader_Code",
                schema: "Sales",
                table: "PromoDetail",
                column: "Code",
                principalSchema: "Sales",
                principalTable: "PromoHeader",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoDetailTier_PromoDetail_PromoDetailId",
                schema: "Sales",
                table: "PromoDetailTier",
                column: "PromoDetailId",
                principalSchema: "Sales",
                principalTable: "PromoDetail",
                principalColumn: "Id");

            migrationBuilder.RenameColumn(
                name: "CurrRate",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo",
                newName: "Rate");

            migrationBuilder.RenameColumn(
                name: "CurrRate",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                newName: "Rate");

            migrationBuilder.RenameColumn(
                name: "CurrRate",
                schema: "Accounting",
                table: "BeginningBalanceAR",
                newName: "Rate");

            migrationBuilder.RenameColumn(
                name: "CurrRate",
                schema: "Accounting",
                table: "BeginningBalanceAP",
                newName: "Rate");

            // Alter view dbo.sp_update_transfer_stock
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
						0/*NettPrice */, BaseUnit, 
						CASE WHEN @stockType = 1 AND @loop = 0 THEN BaseQty * -1 
							 WHEN @stockType = 1 AND @loop = 1 THEN BaseQty
							 WHEN @stockType = 2 AND @loop = 0 THEN BaseQty 
							 WHEN @stockType = 2 AND @loop = 1 THEN BaseQty * -1 END,
						0/*BaseNettPrice */,Code, Id,
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
				SELECT @whCodeFrom, @date, ItemId, UomId, UnitId, Qty * -1, 0/*NettPrice */, BaseUnit, BaseQty * -1, 0/*BaseNettPrice */,
					Code, Id,
					NULL, 'OH',
					'TS'
				FROM #tmp_its

			-- Insert stock mutation transfer stock item detail warehouse to
			INSERT INTO Inventory.StockMutation
				SELECT @whCodeTo, @date, ItemId, UomId, UnitId, Qty, 0/*NettPrice */, BaseUnit, BaseQty, 0/*BaseNettPrice */,
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

            // Alter view dbo.sp_update_stock_mutation_from_adj
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
			SET BaseQty = Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
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

            // Refresh view Accounting.vwBeginningBalanceAP
            sql = "execute sp_refreshview 'Accounting.vwBeginningBalanceAP'";
            migrationBuilder.Sql(sql);

            // Refresh view Accounting.vwBeginningBalanceAR
            sql = "execute sp_refreshview 'Accounting.vwBeginningBalanceAR'";
            migrationBuilder.Sql(sql);

            // Refresh view Accounting.vwBeginningBalanceCreditMemo
            sql = "execute sp_refreshview 'Accounting.vwBeginningBalanceCreditMemo'";
            migrationBuilder.Sql(sql);

            // Refresh view Accounting.vwBeginningBalanceDebitMemo
            sql = "execute sp_refreshview 'Accounting.vwBeginningBalanceDebitMemo'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoDetail_PromoHeader_Code",
                schema: "Sales",
                table: "PromoDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PromoDetailTier_PromoDetail_PromoDetailId",
                schema: "Sales",
                table: "PromoDetailTier");

            migrationBuilder.DropTable(
                name: "PromoSubject",
                schema: "Sales");

            migrationBuilder.DropIndex(
                name: "IX_PromoDetailTier_PromoDetailId",
                schema: "Sales",
                table: "PromoDetailTier");

            migrationBuilder.DropIndex(
                name: "IX_PromoDetail_Code",
                schema: "Sales",
                table: "PromoDetail");

            migrationBuilder.RenameColumn(
                name: "Rate",
                schema: "Accounting",
                table: "BeginningBalanceDebitMemo",
                newName: "CurrRate");

            migrationBuilder.RenameColumn(
                name: "Rate",
                schema: "Accounting",
                table: "BeginningBalanceCreditMemo",
                newName: "CurrRate");

            migrationBuilder.RenameColumn(
                name: "Rate",
                schema: "Accounting",
                table: "BeginningBalanceAR",
                newName: "CurrRate");

            migrationBuilder.RenameColumn(
                name: "Rate",
                schema: "Accounting",
                table: "BeginningBalanceAP",
                newName: "CurrRate");
        }
    }
}
