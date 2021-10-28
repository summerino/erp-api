using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewBBStockDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                newName: "UnitPrice");

            // Alter view Inventory.vwBeginningBalanceStockDetail
            var sql = @"ALTER VIEW [Inventory].[vwBeginningBalanceStockDetail]
AS
	SELECT bb_d.*,
		i.[Name] AS ItemName,
		i.Initial AS ItemInitial,
		i.UomBuyId AS ItemUomBuyId,
		uom_c_b.UnitEquivalent AS ItemUomBuyName,
		i.BuyPrice AS ItemBuyPrice
	FROM Inventory.BeginningBalanceDetail bb_d
	LEFT JOIN Inventory.Item i
		ON i.Id = bb_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_b
		ON uom_c_b.Id = i.UomBuyId";
            migrationBuilder.Sql(sql);

            // Alter procedure dbo.sp_update_stock_mutation_from_bb
            sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_bb]
	@code varchar(17),
	@date date
AS
SET NOCOUNT ON
BEGIN TRY

	SELECT d.*, h.WarehouseCode
	INTO #tmp_bb
	FROM Inventory.BeginningBalanceDetail d
	JOIN Inventory.BeginningBalanceHeader h on d.code = h.code
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

	-- Delete stock mutation that doesn't have in bb item detail
	DELETE Inventory.StockMutation
	WHERE RefCode1 = @code
	AND Src = 'BB'
	AND NOT EXISTS (
		SELECT *
		FROM #tmp_bb
		WHERE Code = RefCode1
		AND Id = RefDetailId1
	)

	-- Update stock mutation that exists in bb item detail
	UPDATE sm
	SET sm.WarehouseCode = #tmp_bb.WarehouseCode,
		sm.[Date] = @date,
		sm.ItemId = #tmp_bb.ItemId,
		sm.UomId = #tmp_bb.UomId,
		sm.UnitId = #tmp_bb.UnitId,
		sm.Qty = #tmp_bb.Qty,
		sm.RefCode2 = NULL
	FROM Inventory.StockMutation sm, #tmp_bb
	WHERE sm.RefCode1 = #tmp_bb.Code
	AND sm.RefDetailId1 = #tmp_bb.Id
	AND sm.Src = 'BB'

	-- Insert stock mutation that doesn't have with bb item detail
	INSERT INTO Inventory.StockMutation
		SELECT 
			WarehouseCode as WarehouseCode, 
			@date as [Date], 
			ItemId as ItemId, 
			UomId as UomId, 
			UnitId as UnitId, 
			Qty as Qty,
			UnitPrice as NettPrice,
			UnitId as BaseUnit, 
			0 as BaseQty,
			0 as BaseNettPrice,
			Code as RefCode1, 
			Id as RefDetailId,
			NULL as RefCode,
			'OH' as [Type],
			'BB' as Src
		FROM #tmp_bb bb
		WHERE NOT EXISTS (
			SELECT Id
			FROM Inventory.StockMutation sm
			WHERE sm.RefCode1 = bb.Code
			AND sm.RefDetailId1 = bb.Id
			AND sm.Src = 'BB'
		) And bb.Qty <> 0
	-- Drop temp tables
	DROP TABLE #tmp_bb
	
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
			UPDATE Inventory.StockMutation SET BaseQty = Qty,BaseUnit = UnitId,BaseNettPrice = NettPrice WHERE Id = @Id			
		END
		ELSE
		BEGIN
			UPDATE Inventory.StockMutation 
			SET BaseQty = Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
			BaseNettPrice = NettPrice / (Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq)),
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
	IF OBJECT_ID('tempdb.dbo.#tmp_bb') IS NOT NULL
		DROP TABLE #tmp_bb
	IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
		DROP TABLE #tmp_old_stk
	IF OBJECT_ID('tempdb.dbo.#tmp_stk') IS NOT NULL
		DROP TABLE #tmp_stk
	
	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                schema: "Inventory",
                table: "BeginningBalanceDetail",
                newName: "Amount");
        }
    }
}
