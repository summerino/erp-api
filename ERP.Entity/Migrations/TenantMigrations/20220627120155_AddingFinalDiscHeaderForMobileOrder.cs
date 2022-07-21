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
	WHERE RefCode1 = @code AND Src = 'DO'

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

			// Alter procedure dbo.sp_get_pi_print_data
			sql = @"ALTER PROCEDURE [dbo].[sp_get_pi_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT pi_h.*,
			dbo.udf_num_to_words_id(pi_h.Total, @centToWord) AS TotalInWord,
			e_r.Initial AS RequestInitial,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			u_c.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseInvoiceHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) pi_h
		LEFT JOIN Purchasing.PurchaseOrderHeader po_h
			ON po_h.Code = pi_h.POCode
		LEFT JOIN General.Employee e_r
			ON e_r.Id = po_h.RequestBy
		LEFT JOIN General.Supplier s
			ON s.Code = pi_h.SupCode
		LEFT JOIN SystemManagement.[User] u_c
			ON u_c.Id = pi_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT pi_d.Id, pi_d.Code, pi_d.RcvCode,
			rcv_d.Id AS RcvDetailId, rcv_d.[LineNo] AS RcvDetailLineNo,
			rcv_d.Qty, rcv_d.UnitPrice, rcv_d.Disc + rcv_d.FinalDiscHeader AS Disc,
			CASE WHEN rcv_d.[Type] = 0 THEN rcv_d.TaxAmount
				ELSE 0 END AS TaxAmount,
			CASE WHEN rcv_d.[Type] = 0 THEN rcv_d.ExemptTaxAmount
				ELSE 0 END AS ExemptTaxAmount,
			CASE WHEN rcv_d.[Type] = 0 THEN rcv_d.Total
				ELSE 0 END AS Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			rcv_d.[Type] AS Sort
		FROM (
			SELECT Id, Code, RcvCode
			FROM Purchasing.PurchaseInvoiceDetail
			WHERE Code = @code
		) pi_d
		LEFT JOIN Purchasing.PurchaseReceiveHeader rcv_h
			ON rcv_h.Code = pi_d.RcvCode
		LEFT JOIN Purchasing.PurchaseReceiveDetail rcv_d
			ON rcv_d.Code = rcv_h.Code
		LEFT JOIN Inventory.Item i
			ON i.Id = rcv_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = rcv_d.UnitId
		ORDER BY Sort, Id, RcvCode, RcvDetailId, RcvDetailLineNo
	END

END";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_get_po_print_data
			sql = @"ALTER PROCEDURE [dbo].[sp_get_po_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT po_h.*,
			dbo.udf_num_to_words_id(po_h.Total, @centToWord) AS TotalInWord,
			e_r.Initial AS RequestInitial,
			s.Initial AS SupInitial, s.[Name] AS SupName, s.Address1 AS SupAddress1, s.Phone AS SupPhone,
			e_c.Initial AS CreatedInitial
		FROM (
			SELECT *
			FROM Purchasing.PurchaseOrderHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) po_h
		LEFT JOIN General.Employee e_r
			ON e_r.Id = po_h.RequestBy
		LEFT JOIN General.Supplier s
			ON s.Code = po_h.SupCode
		LEFT JOIN General.Employee e_c
			ON e_c.Id = po_h.CreatedBy
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT po_d.Id, po_d.[LineNo],
			po_d.Qty, po_d.UnitPrice, po_d.Disc + FinalDiscHeader AS Disc,
			CASE WHEN po_d.[Type] = 0 THEN po_d.TaxAmount
				ELSE 0 END AS TaxAmount,
			CASE WHEN po_d.[Type] = 0 THEN po_d.ExemptTaxAmount
				ELSE 0 END AS ExemptTaxAmount,
			CASE WHEN po_d.[Type] = 0 THEN po_d.Total
				ELSE 0 END AS Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			po_d.[Type] AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, Disc, FinalDiscHeader,
				TaxAmount, ExemptTaxAmount, Total, [Type]
			FROM Purchasing.PurchaseOrderDetail
			WHERE Code = @code
		) po_d
		LEFT JOIN Inventory.Item i
			ON i.Id = po_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = po_d.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

END";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_get_si_print_data
			sql = @"ALTER PROCEDURE [dbo].[sp_get_si_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT si_h.*,
			dbo.udf_num_to_words_id(si_h.Total, @centToWord) AS TotalInWord,
			e.Initial AS SalesInitial,
			c.Initial AS CustInitial, c.[Name] AS CustName,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.ContactPerson
				ELSE ca_b.ContactPerson END AS CustContactPerson
		FROM (
			SELECT *
			FROM Sales.SalesInvoiceHeader
			WHERE Code = @code
			AND Mark <> 'V'
		) si_h
		LEFT JOIN Sales.SalesOrderHeader so_h
			ON so_h.Code = si_h.SOCode
		LEFT JOIN General.Employee e
			ON e.Id = so_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = si_h.CustCode
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = si_h.CustCode
			AND ca_b.Id = so_h.BillingAddressId
			AND so_h.BillingAddressId IS NOT NULL
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = si_h.CustCode
			AND ca_d.IsDefault = 1
			AND so_h.BillingAddressId IS NULL
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT si_d.Id, si_d.Code, si_d.DOCode,
			dlv_d.Id AS DlvDetailId, dlv_d.[LineNo] AS DlvDetailLineNo,
			dlv_d.Qty, dlv_d.UnitPrice, dlv_d.Disc + dlv_d.FinalDiscHeader AS Disc,
			dlv_d.TaxAmount, dlv_d.ExemptTaxAmount, dlv_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, Code, DOCode
			FROM Sales.SalesInvoiceDetail
			WHERE Code = @code
		) si_d
		LEFT JOIN Sales.SalesDeliveryHeader dlv_h
			ON dlv_h.Code = si_d.DOCode
		LEFT JOIN Sales.SalesDeliveryDetail dlv_d
			ON dlv_d.Code = dlv_h.Code
		LEFT JOIN Inventory.Item i
			ON i.Id = dlv_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = dlv_d.UnitId
		UNION ALL
		SELECT 0, '', dlv_d_fg.Code,
			dlv_d_fg.DlvOrderDetailId, dlv_d_fg.[LineNo],
			dlv_d_fg.Qty, dlv_d_fg.UnitPrice, dlv_d_fg.UnitPrice,
			0, 0, 0,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT dlv_d_fg.*
			FROM Sales.SalesDeliveryDetailFreeGood dlv_d_fg
			WHERE EXISTS (
				SELECT si_d.Id, si_d.Code, si_d.DOCode
				FROM Sales.SalesInvoiceDetail si_d
				WHERE si_d.Code = @code
				AND si_d.DOCode = dlv_d_fg.Code
			)
		) dlv_d_fg
		LEFT JOIN Inventory.Item i
			ON i.Id = dlv_d_fg.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = dlv_d_fg.UnitId
		ORDER BY Sort, Id, DOCode, DlvDetailId, DlvDetailLineNo
	END

END";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_get_so_print_data
			sql = @"ALTER PROCEDURE [dbo].[sp_get_so_print_data]
	@code varchar(17),
	@displayType varchar(20) = 'HEADER',
	@centToWord bit = 0
AS
BEGIN

	IF @displayType = 'HEADER'
	BEGIN
		SELECT so_h.*,
			dbo.udf_num_to_words_id(so_h.Total, @centToWord) AS TotalInWord,
			e.Initial AS SalesInitial,
			c.Initial AS CustInitial, c.[Name] AS CustName,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Address1
				ELSE ca_b.Address1 END AS CustAddress1,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.Phone
				ELSE ca_b.Phone END AS CustPhone,
			CASE WHEN so_h.BillingAddressId IS NULL THEN ca_d.ContactPerson
				ELSE ca_b.ContactPerson END AS CustContactPerson
		FROM (
			SELECT *
			FROM Sales.SalesOrderHeader 
			WHERE Code = @code
			AND Mark <> 'V'
		) so_h
		LEFT JOIN General.Employee e
			ON e.Id = so_h.SalesBy
		LEFT JOIN General.Customer c
			ON c.Code = so_h.CustCode
		LEFT JOIN General.CustomerAddress ca_b
			ON ca_b.Code = so_h.CustCode
			AND ca_b.Id = so_h.BillingAddressId
			AND so_h.BillingAddressId IS NOT NULL
		LEFT JOIN General.CustomerAddress ca_d
			ON ca_d.Code = so_h.CustCode
			AND ca_d.IsDefault = 1
			AND so_h.BillingAddressId IS NULL
	END

	ELSE IF @displayType = 'DETAIL'
	BEGIN
		SELECT si_d.Id, si_d.[LineNo],
			si_d.Qty, si_d.UnitPrice, si_d.Disc + si_d.FinalDiscHeader AS Disc,
			si_d.TaxAmount, si_d.ExemptTaxAmount, si_d.Total,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			1 AS Sort
		FROM (
			SELECT Id, Code, [LineNo], ItemId, UnitId, Qty, UnitPrice, Disc, FinalDiscHeader,
				TaxAmount, ExemptTaxAmount, Total
			FROM Sales.SalesOrderDetail
			WHERE Code = @code
		) si_d
		LEFT JOIN Inventory.Item i
			ON i.Id = si_d.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = si_d.UnitId
		UNION ALL
		SELECT so_d_fg.Id, so_d_fg.[LineNo],
			so_d_fg.Qty, so_d_fg.UnitPrice, so_d_fg.UnitPrice,
			0, 0, 0,
			i.Initial AS ItemInitial, i.[Name] AS ItemName,
			uom_c.UnitToConvert AS ItemUnitName,
			2 AS Sort
		FROM (
			SELECT *
			FROM Sales.SalesOrderDetailFreeGood
			WHERE Code = @code
		) so_d_fg
		LEFT JOIN Inventory.Item i
			ON i.Id = so_d_fg.ItemId
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.Id = so_d_fg.UnitId
		ORDER BY Sort, Id, [LineNo]
	END

END";
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
