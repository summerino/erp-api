using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class AddingPaymentTermAndBillingAddressInSalesOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingAddressId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            // Reorder column in Sales.SalesOrderHeader
            var sql = @"BEGIN TRANSACTION
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
CREATE TABLE Sales.Tmp_SalesOrderHeader
	(
	Code varchar(17) NOT NULL,
	Date date NOT NULL,
	CustCode varchar(8) NOT NULL,
	SalesBy bigint NOT NULL,
	BillingAddressId int NULL,
	PaymentTermId int NULL,
	WarehouseCode varchar(8) NULL,
	CurrCode varchar(3) NOT NULL,
	Rate decimal(18, 2) NOT NULL,
	ShipmentFee decimal(18, 2) NOT NULL,
	HandlingFee decimal(18, 2) NOT NULL,
	SubTotal decimal(18, 2) NOT NULL,
	FinalDiscPercent decimal(5, 2) NOT NULL,
	FinalDisc decimal(18, 2) NOT NULL,
	IncludeTax bit NOT NULL,
	TaxAmount decimal(18, 2) NOT NULL,
	Total decimal(18, 2) NOT NULL,
	DPP decimal(18, 2) NOT NULL,
	Notes varchar(256) NULL,
	FromDirectInvoice bit NOT NULL,
	Mark varchar(3) NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL,
	ApprovedBy int NULL,
	ApprovedDate datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE Sales.Tmp_SalesOrderHeader SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM Sales.SalesOrderHeader)
	 EXEC('INSERT INTO Sales.Tmp_SalesOrderHeader (Code, Date, CustCode, SalesBy, BillingAddressId, PaymentTermId, WarehouseCode, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate)
		SELECT Code, Date, CustCode, SalesBy, BillingAddressId, PaymentTermId, WarehouseCode, CurrCode, Rate, ShipmentFee, HandlingFee, SubTotal, FinalDiscPercent, FinalDisc, IncludeTax, TaxAmount, Total, DPP, Notes, FromDirectInvoice, Mark, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate, ApprovedBy, ApprovedDate FROM Sales.SalesOrderHeader WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE Sales.SalesOrderHeader
GO
EXECUTE sp_rename N'Sales.Tmp_SalesOrderHeader', N'SalesOrderHeader', 'OBJECT' 
GO
ALTER TABLE Sales.SalesOrderHeader ADD CONSTRAINT
	PK_SalesOrderHeader PRIMARY KEY CLUSTERED 
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

            // Execute sp_refreshview Sales.vwSalesOrderHeader
            sql = @"exec sp_refreshview 'Sales.vwSalesOrderHeader'";
            migrationBuilder.Sql(sql);

            // Alter procedure in dbo.sp_update_transfer_stock
            sql = @"ALTER PROCEDURE [dbo].[sp_update_transfer_stock]
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

 

    IF (@IsComplete = 0) --Active
    BEGIN
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
            migrationBuilder.DropColumn(
                name: "BillingAddressId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "PaymentTermId",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
