using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingColumnIsConsigneeInTransferStockHeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "varchar(5)",
                unicode: false,
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<bool>(
                name: "IsConsignee",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Update transfer stock type
            var sql = @"UPDATE Inventory.TransferStockHeader
SET Type = 'OUT'
WHERE Type = 1";
            migrationBuilder.Sql(sql);

            sql = @"UPDATE Inventory.TransferStockHeader
SET Type = 'IN'
WHERE Type = 2";
            migrationBuilder.Sql(sql);

            sql = @"UPDATE Inventory.TransferStockHeader
SET Type = 'DT'
WHERE Type = 3";
            migrationBuilder.Sql(sql);

            //// Reorder column Inventory.TransferStockHeader
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
ALTER TABLE Inventory.TransferStockHeader
	DROP CONSTRAINT FK_TransferStockHeader_Warehouse_WarehouseCodeFrom
GO
ALTER TABLE Inventory.TransferStockHeader
	DROP CONSTRAINT FK_TransferStockHeader_Warehouse_WarehouseCodeTo
GO
ALTER TABLE Inventory.Warehouse SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE Inventory.Tmp_TransferStockHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	Type varchar(5) NOT NULL,
	OriginTransferCode varchar(17) NULL,
	WarehouseCodeFrom varchar(8) NULL,
	WarehouseCodeTo varchar(8) NULL,
	Notes varchar(256) NULL,
	IsConsignee bit NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Inventory.Tmp_TransferStockHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Inventory.TransferStockHeader)
	 EXEC('INSERT INTO Inventory.Tmp_TransferStockHeader (Code, Date, Type, OriginTransferCode, WarehouseCodeFrom, WarehouseCodeTo, Notes, IsConsignee, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, Type, OriginTransferCode, WarehouseCodeFrom, WarehouseCodeTo, Notes, IsConsignee, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Inventory.TransferStockHeader WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE Inventory.TransferStockDetail
	DROP CONSTRAINT FK_TransferStockDetail_TransferStockHeader_Code
GO
DROP TABLE Inventory.TransferStockHeader
GO
EXECUTE sp_rename N'Inventory.Tmp_TransferStockHeader', N'TransferStockHeader', 'OBJECT' 
GO
ALTER TABLE Inventory.TransferStockHeader ADD CONSTRAINT
	PK_TransferStockHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_TransferStockHeader_WarehouseCodeFrom ON Inventory.TransferStockHeader
	(
	WarehouseCodeFrom
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_TransferStockHeader_WarehouseCodeTo ON Inventory.TransferStockHeader
	(
	WarehouseCodeTo
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE Inventory.TransferStockHeader ADD CONSTRAINT
	FK_TransferStockHeader_Warehouse_WarehouseCodeFrom FOREIGN KEY
	(
	WarehouseCodeFrom
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.TransferStockHeader ADD CONSTRAINT
	FK_TransferStockHeader_Warehouse_WarehouseCodeTo FOREIGN KEY
	(
	WarehouseCodeTo
	) REFERENCES Inventory.Warehouse
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE Inventory.TransferStockDetail ADD CONSTRAINT
	FK_TransferStockDetail_TransferStockHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES Inventory.TransferStockHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE Inventory.TransferStockDetail SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Refresh view Inventory.vwTransferStockHeader
            sql = @"execute sp_refreshview 'Inventory.vwTransferStockHeader'";
            migrationBuilder.Sql(sql);

            // Create view [Accounting].[vwClosingMonth]
            sql = @"CREATE VIEW [Accounting].[vwClosingMonth]
AS
	SELECT cm.*,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.ClosingMonth cm
	LEFT JOIN SystemManagement.[User] cr
		ON cm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON cm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

            // Alter procedure dbo.sp_update_transfer_stock
            sql = @"ALTER PROCEDURE [dbo].[sp_update_transfer_stock]
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

		IF (@stockType != 'DT') --InventoryIn & Out
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
        ELSE IF (@stockType = 'DT') --DirectTransfer
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
        ELSE IF (@stockType = 'DT')
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
            migrationBuilder.DropColumn(
                name: "IsConsignee",
                schema: "Inventory",
                table: "TransferStockHeader");

            migrationBuilder.AlterColumn<short>(
                name: "Type",
                schema: "Inventory",
                table: "TransferStockHeader",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(5)",
                oldUnicode: false,
                oldMaxLength: 5);

            // Drop view [Accounting].[vwClosingMonth]
            var sql = @"DROP VIEW [Accounting].[vwClosingMonth]";
            migrationBuilder.Sql(sql);
        }
    }
}
