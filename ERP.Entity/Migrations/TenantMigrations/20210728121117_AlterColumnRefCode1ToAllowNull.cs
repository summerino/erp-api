using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterColumnRefCode1ToAllowNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefCode1",
                schema: "Accounting",
                table: "Journal",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_CashBankType_SysParCode",
                schema: "Finance",
                table: "CashBankType",
                column: "SysParCode");

            // Create view Finance.vwCashBankType
            var sql = @"CREATE VIEW [Finance].[vwCashBankType]
AS
	SELECT t.*,
		sp.[Value] AS CoaCode,
		c.[Name] AS CoaName
	FROM Finance.CashBankType t
	LEFT JOIN SystemManagement.SystemParameter sp
		ON sp.Code = t.SysParCode
	LEFT JOIN Accounting.COA c
		ON c.Code = sp.[Value]";
            migrationBuilder.Sql(sql);

            // Alter procedure dbo.sp_update_stock_mutation_from_pr
            sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_pr]
	@code varchar(17),
	@date date,
	@rcvCode varchar(17)
AS
BEGIN TRY

	DECLARE @Type int
	SELECT @Type = [Type] FROM Purchasing.PurchaseReturnHeader WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PR'

	IF (@Type IN (1,2))
	BEGIN
		SELECT pr_d.*,
			CASE WHEN uom_c.IsBaseUnit = 1 THEN pr_d.UnitId
				ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
			CASE WHEN uom_c.IsBaseUnit = 1 THEN pr_d.Qty
				ELSE pr_d.Qty * (
					SELECT EXP(SUM(LOG(Conversion)))
					FROM Inventory.UoMConversion
					WHERE UomId = uom_c.UomId
					AND Seq <= uom_c.Seq
				) END AS BaseQty
		INTO #tmp_pr
		FROM Purchasing.PurchaseReturnDetail pr_d
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.UomId = pr_d.UomId
			AND uom_c.Id = pr_d.UnitId
		WHERE Code = @code

		-- Delete stock mutation that doesn't have in purchase receive item detail
		DELETE Inventory.StockMutation
		WHERE RefCode1 = @code
		AND Src = 'PR'
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
			sm.RefCode2 = CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END
		FROM Inventory.StockMutation sm, #tmp_pr
		WHERE sm.RefCode1 = #tmp_pr.Code
		AND sm.RefDetailId1 = #tmp_pr.Id
		AND sm.Src = 'PR'

		-- Insert stock mutation that doesn't have with purchase receive item detail
		INSERT INTO Inventory.StockMutation
			SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id,
				CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END, 'OH',
				'PR'
			FROM #tmp_pr pr
			WHERE NOT EXISTS (
				SELECT Id
				FROM Inventory.StockMutation sm
				WHERE sm.RefCode1 = pr.Code
				AND sm.RefDetailId1 = pr.Id
				AND sm.Src = 'PR'
			)
	END
	ELSE IF (@Type = 3)
	BEGIN
		SELECT pr_d.*,
			CASE WHEN uom_c.IsBaseUnit = 1 THEN pr_d.UnitId
				ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
			CASE WHEN uom_c.IsBaseUnit = 1 THEN pr_d.Qty
				ELSE pr_d.Qty * (
					SELECT EXP(SUM(LOG(Conversion)))
					FROM Inventory.UoMConversion
					WHERE UomId = uom_c.UomId
					AND Seq <= uom_c.Seq
				) END AS BaseQty
		INTO #tmp_prx
		FROM Purchasing.PurchaseReturnDetailExchDiffItem pr_d
		LEFT JOIN Inventory.UoMConversion uom_c
			ON uom_c.UomId = pr_d.UomId
			AND uom_c.Id = pr_d.UnitId
		WHERE Code = @code

		-- Delete stock mutation that doesn't have in purchase receive item detail
		DELETE Inventory.StockMutation
		WHERE RefCode1 = @code
		AND Src = 'PR'
		AND NOT EXISTS (
			SELECT *
			FROM #tmp_prx
			WHERE Code = RefCode1
			AND Id = RefDetailId1
		)

		-- Update stock mutation that exists in purchase receive item detail
		UPDATE sm
		SET sm.WarehouseCode = #tmp_prx.WarehouseCode,
			sm.[Date] = @date,
			sm.ItemId = #tmp_prx.ItemId,
			sm.UomId = #tmp_prx.UomId,
			sm.UnitId = #tmp_prx.UnitId,
			sm.Qty = #tmp_prx.Qty,
			sm.BaseUnit = #tmp_prx.BaseUnit,
			sm.BaseQty = #tmp_prx.BaseQty,
			sm.RefCode2 = CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END
		FROM Inventory.StockMutation sm, #tmp_prx
		WHERE sm.RefCode1 = #tmp_prx.Code
		AND sm.RefDetailId1 = #tmp_prx.Id
		AND sm.Src = 'PR'

		-- Insert stock mutation that doesn't have with purchase receive item detail
		INSERT INTO Inventory.StockMutation
			SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, 0, BaseUnit, BaseQty, 0, Code, Id,
				CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END, 'OH',
				'PR'
			FROM #tmp_prx pr
			WHERE NOT EXISTS (
				SELECT Id
				FROM Inventory.StockMutation sm
				WHERE sm.RefCode1 = pr.Code
				AND sm.RefDetailId1 = pr.Id
				AND sm.Src = 'PR'
			)
	END

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
			
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @OldQty, UpdatedDate = GETDATE() WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId

			DELETE #tmp_ori_sm WHERE WarehouseCode = @OldWhId AND ItemId = @OldItemId
		END
	END

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
		ELSE
		BEGIN
			INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
			VALUES (@WHId,@ItemId,-@Qty,0,0,0,0,GETDATE())
		END

		DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
	END

	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_prx') IS NOT NULL
		DROP TABLE #tmp_prx
	DROP TABLE #tmp_wq
	DROP TABLE #tmp_ori_sm

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_prx') IS NOT NULL
		DROP TABLE #tmp_prx
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ori_sm') IS NOT NULL
		DROP TABLE #tmp_ori_sm

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_update_stock_mutation_from_sr
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_sr]
	@code varchar(17),
	@date date,
	@dlvCode varchar(17),
	@warehouseCode varchar(20)
AS
BEGIN TRY

	DECLARE @Type int
	SELECT @Type = [Type] FROM Sales.SalesReturnHeader WHERE Code = @code

	-- Collect original stock mutation
	SELECT *
	INTO #tmp_ori_sm
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SR'

	IF (@Type IN (1,2))
	BEGIN
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
	END
	ELSE IF (@Type = 3)
	BEGIN
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
		INTO #tmp_srx
		FROM Sales.SalesReturnDetailExchDiffItem sr_d
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
			FROM #tmp_srx
			WHERE Code = RefCode1
			AND Id = RefDetailId1
		)

		-- Update stock mutation that exists in sales receive item detail
		UPDATE sm
		SET sm.WarehouseCode = @warehouseCode,
			sm.[Date] = @date,
			sm.ItemId = #tmp_srx.ItemId,
			sm.UomId = #tmp_srx.UomId,
			sm.UnitId = #tmp_srx.UnitId,
			sm.Qty = #tmp_srx.Qty,
			sm.NettPrice = #tmp_srx.NettPrice,
			sm.BaseUnit = #tmp_srx.BaseUnit,
			sm.BaseQty = #tmp_srx.BaseQty,
			sm.BaseNettPrice = #tmp_srx.BaseNettPrice,
			sm.RefCode2 = CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END
		FROM Inventory.StockMutation sm, #tmp_srx
		WHERE sm.RefCode1 = #tmp_srx.Code
		AND sm.RefDetailId1 = #tmp_srx.Id
		AND sm.Src = 'SR'

		-- Insert stock mutation that doesn't have with sales receive item detail
		INSERT INTO Inventory.StockMutation
			SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, NettPrice, BaseUnit, BaseQty, BaseNettPrice, Code, Id,
				CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END, 'OH',
				'SR'
			FROM #tmp_srx sr
			WHERE NOT EXISTS (
				SELECT Id
				FROM Inventory.StockMutation sm
				WHERE sm.RefCode1 = sr.Code
				AND sm.RefDetailId1 = sr.Id
				AND sm.Src = 'SR'
			)
	END
	

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

			// Alter view Accounting.vwClosingMonth
			sql = @"ALTER VIEW [Accounting].[vwClosingMonth]
AS
	SELECT cm.*,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial,
		(Select DateName( month , DateAdd( month , CAST((select RIGHT(cm.Period,2)) as INT) , -1 ) )+ '-' + (select LEFT(cm.Period,4))) AS PeriodName
	FROM Accounting.ClosingMonth cm
	LEFT JOIN SystemManagement.[User] cr
		ON cm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON cm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CashBankType_SysParCode",
                schema: "Finance",
                table: "CashBankType");

            migrationBuilder.AlterColumn<string>(
                name: "RefCode1",
                schema: "Accounting",
                table: "Journal",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldUnicode: false,
                oldMaxLength: 20,
                oldNullable: true);

            // Drop view Finance.vwCashBankType
            var sql = @"DROP VIEW [Finance].[vwCashBankType]";
            migrationBuilder.Sql(sql);
        }
    }
}
