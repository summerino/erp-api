using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AddingFinalDiscHeaderForMobileOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinalDiscHeader",
                schema: "MobileSales",
                table: "MobileOrderDetail",
                type: "decimal(19,6)",
                precision: 19,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

			// Reorder column MobileSales.MobileOrderDetail
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
ALTER TABLE MobileSales.MobileOrderDetail
	DROP CONSTRAINT FK_MobileOrderDetail_MobileOrderHeader_Code
GO
ALTER TABLE MobileSales.MobileOrderHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetail
	DROP CONSTRAINT FK_MobileOrderDetail_Tax_TaxId
GO
ALTER TABLE General.Tax SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetail
	DROP CONSTRAINT FK_MobileOrderDetail_UoMConversion_UnitId
GO
ALTER TABLE Inventory.UoMConversion SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetail
	DROP CONSTRAINT FK_MobileOrderDetail_UoM_UomId
GO
ALTER TABLE Inventory.UoM SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetail
	DROP CONSTRAINT FK_MobileOrderDetail_Item_ItemId
GO
ALTER TABLE Inventory.Item SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE MobileSales.Tmp_MobileOrderDetail
	(
	Id bigint NOT NULL IDENTITY (1, 1),
	Code varchar(17) NOT NULL,
	[LineNo] smallint NOT NULL,
	ItemId int NOT NULL,
	UomId int NOT NULL,
	UnitId int NOT NULL,
	Qty decimal(18, 2) NOT NULL,
	UnitPrice decimal(19, 6) NOT NULL,
	Disc decimal(19, 6) NOT NULL,
	FinalDiscHeader decimal(19, 6) NOT NULL,
	TaxId int NULL,
	TaxAmount decimal(19, 6) NOT NULL,
	ExemptTaxAmount decimal(19, 6) NOT NULL,
	NettPrice decimal(19, 6) NOT NULL,
	Total decimal(19, 6) NOT NULL,
	DPP decimal(19, 6) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE MobileSales.Tmp_MobileOrderDetail SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT MobileSales.Tmp_MobileOrderDetail ON
GO
IF EXISTS(SELECT * FROM MobileSales.MobileOrderDetail)
	 EXEC('INSERT INTO MobileSales.Tmp_MobileOrderDetail (Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP)
		SELECT Id, Code, [LineNo], ItemId, UomId, UnitId, Qty, UnitPrice, Disc, FinalDiscHeader, TaxId, TaxAmount, ExemptTaxAmount, NettPrice, Total, DPP FROM MobileSales.MobileOrderDetail WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT MobileSales.Tmp_MobileOrderDetail OFF
GO
ALTER TABLE MobileSales.MobileOrderDetailDiscount
	DROP CONSTRAINT FK_MobileOrderDetailDiscount_MobileOrderDetail_OrderDetailId
GO
ALTER TABLE MobileSales.MobileOrderDetailFreeGood
	DROP CONSTRAINT FK_MobileOrderDetailFreeGood_MobileOrderDetail_OrderDetailId
GO
DROP TABLE MobileSales.MobileOrderDetail
GO
EXECUTE sp_rename N'MobileSales.Tmp_MobileOrderDetail', N'MobileOrderDetail', 'OBJECT' 
GO
ALTER TABLE MobileSales.MobileOrderDetail ADD CONSTRAINT
	PK_MobileOrderDetail PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_MobileOrderDetail_Code ON MobileSales.MobileOrderDetail
	(
	Code
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileOrderDetail_ItemId ON MobileSales.MobileOrderDetail
	(
	ItemId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileOrderDetail_TaxId ON MobileSales.MobileOrderDetail
	(
	TaxId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileOrderDetail_UnitId ON MobileSales.MobileOrderDetail
	(
	UnitId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_MobileOrderDetail_UomId ON MobileSales.MobileOrderDetail
	(
	UomId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE MobileSales.MobileOrderDetail ADD CONSTRAINT
	FK_MobileOrderDetail_Item_ItemId FOREIGN KEY
	(
	ItemId
	) REFERENCES Inventory.Item
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderDetail ADD CONSTRAINT
	FK_MobileOrderDetail_UoM_UomId FOREIGN KEY
	(
	UomId
	) REFERENCES Inventory.UoM
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderDetail ADD CONSTRAINT
	FK_MobileOrderDetail_UoMConversion_UnitId FOREIGN KEY
	(
	UnitId
	) REFERENCES Inventory.UoMConversion
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderDetail ADD CONSTRAINT
	FK_MobileOrderDetail_Tax_TaxId FOREIGN KEY
	(
	TaxId
	) REFERENCES General.Tax
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderDetail ADD CONSTRAINT
	FK_MobileOrderDetail_MobileOrderHeader_Code FOREIGN KEY
	(
	Code
	) REFERENCES MobileSales.MobileOrderHeader
	(
	Code
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetailFreeGood ADD CONSTRAINT
	FK_MobileOrderDetailFreeGood_MobileOrderDetail_OrderDetailId FOREIGN KEY
	(
	OrderDetailId
	) REFERENCES MobileSales.MobileOrderDetail
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderDetailFreeGood SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE MobileSales.MobileOrderDetailDiscount ADD CONSTRAINT
	FK_MobileOrderDetailDiscount_MobileOrderDetail_OrderDetailId FOREIGN KEY
	(
	OrderDetailId
	) REFERENCES MobileSales.MobileOrderDetail
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE MobileSales.MobileOrderDetailDiscount SET (LOCK_ESCALATION = TABLE)
GO
COMMIT";
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id, @transCode,'OH', 'DO'
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id, @transCode,'OO', 'DO'
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id, @transCode,'OH', 'DOF'
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id, @transCode,'OO', 'DOF'
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
	SELECT WarehouseCode, ItemId, BaseQty, [Type], UnitId
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @srcTrans int
	DECLARE @Type varchar(max)
	DECLARE @UnitId varchar(max)

	SELECT @srcTrans = SrcTrans FROM Sales.SalesDeliveryHeader WHERE Code = @code

	IF(@isVoid = 0)
	BEGIN
		--Update WHQ
		WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId, @Type = [Type] FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				IF (@srcTrans = 1)
				BEGIN
					IF (@Type = 'OH')
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					END
					ELSE IF (@Type = 'OO')
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnOrder = QtyOnOrder - @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId AND UnitId = @UnitId) AND ItemId = @ItemId
					END
					ELSE
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnTransit = QtyOnTransit + @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					END
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode, ItemId, QtyOnHand, QtyOnIndent, QtyOnOrder, QtyReorderPoint, QtyOnTransfer, QtyOnTransit, UpdatedDate)
				VALUES (@WHId, @ItemId, 0, 0, 0, 0, 0, 0, dbo.udf_current_local_time())
				IF (@srcTrans = 1)
				BEGIN
					IF (@Type = 'OH')
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					END
					ELSE IF (@Type = 'OO')
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnOrder = QtyOnOrder - @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId AND UnitId = @UnitId) AND ItemId = @ItemId
					END
					ELSE
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnTransit = QtyOnTransit + @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					END
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand - @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId AND [Type] = @Type
		END
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @UnitId = UnitId, @Type = [Type] FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code AND UnitId = @UnitId AND [Type] = @Type)
			BEGIN
				IF (@srcTrans = 1)
				BEGIN
					IF (@Type = 'OH')
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					END
					ELSE IF (@Type = 'OO')
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnOrder = QtyOnOrder + @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = (SELECT TOP 1 WarehouseCode FROM Inventory.StockMutation WHERE RefCode1 = @transCode AND ItemId = @ItemId AND UnitId = @UnitId) AND ItemId = @ItemId
					END
					ELSE
					BEGIN
						UPDATE Inventory.WarehouseQuantity SET QtyOnTransit = QtyOnTransit - @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
					END
				END
				ELSE
				BEGIN
					UPDATE Inventory.WarehouseQuantity SET QtyOnHand = QtyOnHand + @Qty, UpdatedDate = dbo.udf_current_local_time() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
				END
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code AND UnitId = @UnitId AND [Type] = @Type
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND UnitId = @UnitId AND [Type] = @Type
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

			// Refresh view MobileSales.vwMobileOrderDetail
			sql = @"EXEC sp_refreshview 'MobileSales.vwMobileOrderDetail'";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalDiscHeader",
                schema: "MobileSales",
                table: "MobileOrderDetail");
        }
    }
}
