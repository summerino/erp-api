using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterProcStockMutation3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Alter procedure dbo.sp_update_stock_mutation_from_rcv
			var sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_rcv]
	@code varchar(17),
	@date date,
	@transCode varchar(17),
	@isVoid bit = 0
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
			) END AS BaseQty,
		(pr_d.NettPrice - (CASE WHEN pr_h.FinalDisc > 0 THEN pr_h.FinalDisc * pr_d.NettPrice / (SELECT SUM(NettPrice) FROM Purchasing.PurchaseReceiveDetail WHERE Code = @code) ELSE 0 END)) - pr_d.TaxAmount 
		AS FinalNettPrice,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN
			(pr_d.NettPrice - (CASE WHEN pr_h.FinalDisc > 0 THEN pr_h.FinalDisc * pr_d.NettPrice / (SELECT SUM(NettPrice) FROM Purchasing.PurchaseReceiveDetail WHERE Code = @code) ELSE 0 END)) - pr_d.TaxAmount
		ELSE
			((pr_d.NettPrice - (CASE WHEN pr_h.FinalDisc > 0 THEN pr_h.FinalDisc * pr_d.NettPrice / (SELECT SUM(NettPrice) FROM Purchasing.PurchaseReceiveDetail WHERE Code = @code) ELSE 0 END)) - pr_d.TaxAmount) / 
			(
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			)
		END AS BaseNettPrice
	INTO #tmp_pr
	FROM Purchasing.PurchaseReceiveDetail pr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = pr_d.UomId
		AND uom_c.Id = pr_d.UnitId
	LEFT JOIN Purchasing.PurchaseReceiveHeader pr_h
		ON pr_h.Code = pr_d.Code
	WHERE pr_d.Code = @code

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
		sm.RefCode2 = CASE WHEN @transCode IS NOT NULL THEN @transCode ELSE NULL END,
		sm.BaseNettPrice = #tmp_pr.BaseNettPrice,
		sm.NettPrice = #tmp_pr.FinalNettPrice
	FROM Inventory.StockMutation sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'RCV'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, FinalNettPrice, BaseUnit, BaseQty, BaseNettPrice,
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

	IF(@isVoid = 0)
	BEGIN
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
	END
	ELSE -- If data voided
	BEGIN
	WHILE EXISTS(SELECT * FROM #tmp_wq)
		BEGIN
			SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

			IF EXISTS(SELECT *FROM Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code)
			BEGIN
				DELETE Inventory.StockMutation WHERE WarehouseCode = @WHId AND ItemId = @ItemId AND RefCode1 = @code
			END
			DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
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

			// Create view MobileWarehouse.vwMobileTransferStockHeader
			sql = @"CREATE VIEW [MobileWarehouse].[vwMobileTransferStockHeader]
AS
    SELECT mt_h.*,
        u_c.Initial AS CreatedInitial,
        u_u.Initial AS UpdatedInitial,
        u_a.Initial AS ApprovedInitial,
        u_r.Initial AS RejectedInitial,
        CASE mt_h.Mark
            WHEN 'A' THEN 'Aktif'
            WHEN 'APR' THEN 'Disetujui'
            WHEN 'REJ' THEN 'Ditolak' END AS [Status]
    FROM MobileWarehouse.MobileTransferStockHeader mt_h
    LEFT JOIN SystemManagement.[User] u_c
        ON u_c.Id = mt_h.CreatedBy
    LEFT JOIN SystemManagement.[User] u_u
        ON u_u.Id = mt_h.UpdatedBy
    LEFT JOIN SystemManagement.[User] u_a
        ON u_a.Id = mt_h.ApprovedBy
	LEFT JOIN SystemManagement.[User] u_r
        ON u_r.Id = mt_h.RejectedBy";
            migrationBuilder.Sql(sql);

			// Create view MobileWarehouse.vwMobileTransferStockDetail
			sql = @"CREATE VIEW [MobileWarehouse].[vwMobileTransferStockDetail]
AS
	SELECT mt_d.*,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM MobileWarehouse.MobileTransferStockDetail mt_d
	LEFT JOIN Inventory.Item i
		ON i.Id = mt_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = mt_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = mt_d.UnitId";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
		{
			// Drop view MobileWarehouse.vwMobilePaymentMethod
			var sql = @"DROP VIEW [MobileWarehouse].[vwMobileTransferStockHeader]";
            migrationBuilder.Sql(sql);

			// Drop view MobileWarehouse.vwMobilePaymentMethod
			sql = @"DROP VIEW [MobileWarehouse].[vwMobileTransferStockDetail]";
            migrationBuilder.Sql(sql);
		}
    }
}
