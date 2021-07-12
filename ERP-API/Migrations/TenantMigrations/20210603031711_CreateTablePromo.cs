using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class CreateTablePromo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromoDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ApplyTo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    PromoType = table.Column<short>(type: "smallint", nullable: false),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    ValuePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ValueAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPromoWithBudget = table.Column<bool>(type: "bit", nullable: false),
                    BudgetMaximumValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverBudgetAction = table.Column<short>(type: "smallint", nullable: false),
                    SubGroup1 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup2 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup3 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup4 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SubGroup5 = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromoDetailTier",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromoDetailId = table.Column<long>(type: "bigint", nullable: false),
                    FromQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ToQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsPercentage = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaleUnit = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    ApplyToAllUnit = table.Column<bool>(type: "bit", nullable: false),
                    FreeGoodItemId = table.Column<int>(type: "int", nullable: true),
                    UnitFreeGood = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    IsMultiple = table.Column<bool>(type: "bit", nullable: false),
                    PaymentTermId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoDetailTier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromoHeader",
                schema: "Sales",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    CoaCost = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoHeader", x => x.Code);
                });

			// Alter procedure dbo.sp_update_stock_mutation_from_pr
			var sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_pr]
	@code varchar(17),
	@date date,
	@rcvCode varchar(17)
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
			) END AS BaseQty
	INTO #tmp_pr
	FROM Purchasing.PurchaseReturnDetail pr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = pr_d.UomId
		AND uom_c.Id = pr_d.UnitId
	WHERE Code = @code

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
	INTO #tmp_ex
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
		FROM #tmp_pr
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Delete stock mutation that doesn't have in purchase receive exchange item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'PRX'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_ex
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
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id,
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

		-- Update stock mutation that exists in purchase receive exchange item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_ex.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_ex.ItemId,
		sm.UomId = #tmp_ex.UomId,
		sm.UnitId = #tmp_ex.UnitId,
		sm.Qty = #tmp_ex.Qty,
		sm.BaseUnit = #tmp_ex.BaseUnit,
		sm.BaseQty = #tmp_ex.BaseQty,
		sm.RefCode2 = CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_ex
	WHERE sm.RefCode1 = #tmp_ex.Code
	AND sm.RefDetailId1 = #tmp_ex.Id
	AND sm.Src = 'PRX'

	-- Insert stock mutation that doesn't have with purchase receive exchange item detail
	INSERT INTO Inventory.StockMutation
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id,
			CASE WHEN @rcvCode IS NOT NULL THEN @rcvCode ELSE NULL END, 'OH',
			'PRX'
		FROM #tmp_ex pr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = pr.Code
			AND sm.RefDetailId1 = pr.Id
			AND sm.Src = 'PRX'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty, Src
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @Src varchar(max)

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @Src = Src FROM #tmp_wq


		IF (@Src = 'PR')
			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
				VALUES (@WHId,@ItemId,-@Qty,0,0,0,0,GETDATE())
			END
		ELSE
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
	DROP TABLE #tmp_pr
	DROP TABLE #tmp_ex
	DROP TABLE #tmp_wq

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ex') IS NOT NULL
		DROP TABLE #tmp_ex

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

	SELECT sr_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.Qty
			ELSE sr_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_sr
	FROM Sales.SalesReturnDetail sr_d
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.UomId = sr_d.UomId
		AND uom_c.Id = sr_d.UnitId
	WHERE Code = @code

	SELECT sr_d.*,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.UnitId
			ELSE (SELECT TOP 1 Id FROM Inventory.UoMConversion WHERE UomId = uom_c.UomId AND IsBaseUnit = 1) END AS BaseUnit,
		CASE WHEN uom_c.IsBaseUnit = 1 THEN sr_d.Qty
			ELSE sr_d.Qty * (
				SELECT EXP(SUM(LOG(Conversion)))
				FROM Inventory.UoMConversion
				WHERE UomId = uom_c.UomId
				AND Seq <= uom_c.Seq
			) END AS BaseQty
	INTO #tmp_ex
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
		FROM #tmp_sr
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Delete stock mutation that doesn't have in sales receive exchange item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'SRX'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_ex
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
		sm.BaseUnit = #tmp_sr.BaseUnit,
		sm.BaseQty = #tmp_sr.BaseQty,
		sm.RefCode2 = CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_sr
	WHERE sm.RefCode1 = #tmp_sr.Code
	AND sm.RefDetailId1 = #tmp_sr.Id
	AND sm.Src = 'SR'

	-- Insert stock mutation that doesn't have with sales receive item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id,
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

		-- Update stock mutation that exists in sales receive exchange item detail
	UPDATE sm
	SET sm.WarehouseCode = @warehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_ex.ItemId,
		sm.UomId = #tmp_ex.UomId,
		sm.UnitId = #tmp_ex.UnitId,
		sm.Qty = #tmp_ex.Qty,
		sm.BaseUnit = #tmp_ex.BaseUnit,
		sm.BaseQty = #tmp_ex.BaseQty,
		sm.RefCode2 = CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END
	FROM Inventory.StockMutation sm, #tmp_ex
	WHERE sm.RefCode1 = #tmp_ex.Code
	AND sm.RefDetailId1 = #tmp_ex.Id
	AND sm.Src = 'SRX'

	-- Insert stock mutation that doesn't have with sales receive exchange item detail
	INSERT INTO Inventory.StockMutation
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id,
			CASE WHEN @dlvCode IS NOT NULL THEN @dlvCode ELSE NULL END, 'OH',
			'SRX'
		FROM #tmp_ex sr
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = sr.Code
			AND sm.RefDetailId1 = sr.Id
			AND sm.Src = 'SRX'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty, Src
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int
	DECLARE @Src varchar(max)

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty, @Src = Src FROM #tmp_wq


		IF (@Src = 'SR')
			IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
			BEGIN
				UPDATE Inventory.WarehouseQuantity SET QtyOnHand += @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
			END
			ELSE
			BEGIN
				INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
				VALUES (@WHId,@ItemId,@Qty,0,0,0,0,GETDATE())
			END
		ELSE
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
	DROP TABLE #tmp_sr
	DROP TABLE #tmp_ex
	DROP TABLE #tmp_wq

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_sr') IS NOT NULL
		DROP TABLE #tmp_sr
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq
	IF OBJECT_ID('tempdb.dbo.#tmp_ex') IS NOT NULL
		DROP TABLE #tmp_ex

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_update_stock_mutation_from_do
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_do]
	@code varchar(17),
	@date date,
	@transCode varchar(17)
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
		SELECT @warehouseCode, @date, ItemId, UomId, UnitId, Qty, BaseUnit, BaseQty, Code, Id, @transCode,'OH', 'DO'
		FROM #tmp_do do
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = do.Code
			AND sm.RefDetailId1 = do.Id
			AND sm.Src = 'DO'
		)

	-- Update WarehouseQty
	SELECT WarehouseCode, ItemId, BaseQty
	INTO #tmp_wq
	FROM Inventory.StockMutation
	WHERE RefCode1 = @code

	DECLARE @Qty decimal
	DECLARE @WHId varchar(max)
	DECLARE @ItemId int

	WHILE EXISTS(SELECT * FROM #tmp_wq)
	BEGIN
		SELECT TOP 1 @WHId = WarehouseCode, @ItemId = ItemId, @Qty = BaseQty FROM #tmp_wq

		IF EXISTS(SELECT *FROM Inventory.WarehouseQuantity WHERE WarehouseCode = @WHId AND ItemId = @ItemId)
		BEGIN
			UPDATE Inventory.WarehouseQuantity SET QtyOnHand -= @Qty, QtyOnOrder -= @Qty, UpdatedDate = GETDATE() WHERE WarehouseCode = @WHId AND ItemId = @ItemId
		END
		ELSE
		BEGIN
			INSERT INTO Inventory.WarehouseQuantity(WarehouseCode,ItemId,QtyOnHand,QtyOnIndent,QtyOnOrder,QtyReorderPoint,QtyOnTransfer,UpdatedDate)
			VALUES (@WHId,@ItemId,-@Qty,0,-@Qty,0,0,GETDATE())
		END
		DELETE #tmp_wq WHERE WarehouseCode = @WHId AND ItemId = @ItemId
	END


	-- Drop temp tables
	DROP TABLE #tmp_do
	DROP TABLE #tmp_wq

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_wq') IS NOT NULL
		DROP TABLE #tmp_wq

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromoDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "PromoDetailTier",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "PromoHeader",
                schema: "Sales");
        }
    }
}
