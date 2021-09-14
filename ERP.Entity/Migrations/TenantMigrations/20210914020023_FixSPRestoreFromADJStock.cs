using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class FixSPRestoreFromADJStock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER PROCEDURE [dbo].[sp_restore_stock_mutation_from_adj]
	@code varchar(17),
	@date date
AS
SET NOCOUNT ON
BEGIN TRY

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
		DELETE Inventory.StockMutation Where Id = @Id
		DELETE #tmp_old_stk WHERE Id = @Id
	END
	drop table #tmp_old_stk
	
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
		DROP TABLE #tmp_old_stk
	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER PROCEDURE [dbo].[sp_restore_stock_mutation_from_adj]
	@code varchar(17),
	@date date
AS
SET NOCOUNT ON
BEGIN TRY

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
	
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
		DROP TABLE #tmp_old_stk
	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH
";
            migrationBuilder.Sql(sql);
        }
    }
}
