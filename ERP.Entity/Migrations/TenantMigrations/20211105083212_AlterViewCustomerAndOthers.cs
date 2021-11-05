using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewCustomerAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Alter view General.vwCustomer
			var sql = @"ALTER VIEW [General].[vwCustomer]
AS
	SELECT c.*, 
		t.[Name] AS TypeName,
		ca.Initial AS InitialAddress, ca.Address1, ca.Address2, 
		ca.ContactPerson, ca.Phone, ca.Fax, ca.Lat, ca.Lng,
		ISNULL(ca.IsDefault, 0) AS IsDefault,
		a1.[Name] as AreaName1,
		a2.[Name] as AreaName2,
		a3.[Name] as AreaName3,
		a4.[Name] as AreaName4,
		a5.[Name] as AreaName5,
		u.Initial as UpdatedInitial
	FROM General.Customer c
	LEFT JOIN General.CustomerType t
		ON t.Id = c.TypeId
	LEFT JOIN General.CustomerAddress ca
		ON c.Code = ca.Code
		AND ca.IsDefault = 1
	LEFT JOIN Sales.Area A1
		ON c.AreaId1 = a1.Id
	LEFT JOIN Sales.Area A2
		ON c.AreaId2 = a2.Id
	LEFT JOIN Sales.Area A3
		ON c.AreaId3 = a3.Id
	LEFT JOIN Sales.Area A4
		ON c.AreaId4 = a4.Id
	LEFT JOIN Sales.Area A5
		ON c.AreaId5 = a5.Id
	LEFT JOIN SystemManagement.[User] u
		ON u.Id = c.UpdatedBy";
            migrationBuilder.Sql(sql);

			// Alter view Finance.vwGeneralCashBankDetail
			sql = @"ALTER VIEW [Finance].[vwGeneralCashBankDetail]
AS
	SELECT d.*,
		c.[Name] AS CoaName
	FROM Finance.GeneralCashBankDetail d
	LEFT JOIN Accounting.COA c
		ON c.Code = d.CoaCode";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_update_stock_mutation_from_sr
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_sr]
	@code varchar(17),
	@date date,
	@dlvCode varchar(17),
	@warehouseCode varchar(20)
AS
BEGIN TRY

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SR'

	SELECT sr_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.Qty
			ELSE sr_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.NettPrice
			ELSE sr_d.NettPrice / (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseNettPrice
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
		sm.NettPrice = #tmp_sr.NettPrice,
		sm.BaseUnit = #tmp_sr.BaseUnit,
		sm.BaseQty = #tmp_sr.BaseQty,
		sm.BaseNettPrice = #tmp_sr.BaseNettPrice,
		sm.RefCode2 = CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_sr
	WHERE sm.RefCode1 = #tmp_sr.Code
	AND sm.RefDetailId1 = #tmp_sr.Id
	AND sm.Src = 'SR'

	-- Insert stock mutation that doesn't have with sales receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, NettPrice, BaseUnit, BaseQty, BaseNettPrice, Code, Id,
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
        }
    }
}
