using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Web.API.Migrations.TenantMigrations
{
    public partial class AlterSPStockMutationFromAdj : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
					ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_adj]
						@code varchar(17),
						@date date
					AS
					SET NOCOUNT ON
					BEGIN TRY

						SELECT d.*, h.WarehouseCode
						INTO #tmp_adj
						FROM Inventory.AdjustmentDetail d
						JOIN Inventory.AdjustmentHeader h on d.code = h.code
						WHERE d.Code = @code 

						-- Restore data warehouse qty from stock mutation
						SELECT *
						INTO #tmp_old_stk
						FROM Inventory.StockMutation 
						where RefCode1 = @code

						DECLARE @Id int
						DECLARE @UomId int
						DECLARE @UnitId int
						DECLARE @IsBaseUnit int
						DECLARE @BaseUnitQty int
						DECLARE @Seq int
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

						-- Delete stock mutation that doesn't have in adjustment item detail
						DELETE Inventory.StockMutation
						WHERE RefCode1 = @code
						AND Src = 'ADJ'
						AND NOT EXISTS (
							SELECT *
							FROM #tmp_adj
							WHERE Code = RefCode1
							AND Id = RefDetailId1
						)

						-- Update stock mutation that exists in adjustment item detail
						UPDATE sm
						SET sm.WarehouseCode = #tmp_adj.WarehouseCode,
							sm.[Date] = @date,
							sm.ItemId = #tmp_adj.ItemId,
							sm.UomId = #tmp_adj.UomId,
							sm.UnitId = #tmp_adj.UnitId,
							sm.Qty = #tmp_adj.QtyAdjust,
							sm.RefCode2 = NULL
						FROM Inventory.StockMutation sm, #tmp_adj
						WHERE sm.RefCode1 = #tmp_adj.Code
						AND sm.RefDetailId1 = #tmp_adj.Id
						AND sm.Src = 'ADJ'

						-- Insert stock mutation that doesn't have with adjustment item detail
						INSERT INTO Inventory.StockMutation
							SELECT 
								WarehouseCode, 
								@date, 
								ItemId, 
								UomId, 
								UnitId, 
								QtyAdjust,
								0,
								0,
								Code, 
								Id,
								NULL,
								'OH',
								'ADJ'
							FROM #tmp_adj adj
							WHERE NOT EXISTS (
								SELECT Id
								FROM Inventory.StockMutation sm
								WHERE sm.RefCode1 = adj.Code
								AND sm.RefDetailId1 = adj.Id
								AND sm.Src = 'ADJ'
							) And adj.QtyAdjust <> 0
						-- Drop temp tables
						DROP TABLE #tmp_adj
	
						SELECT *
						INTO #tmp_stk
						FROM Inventory.StockMutation WHERE RefCode1 = @code
	
						WHILE EXISTS(SELECT * FROM #tmp_stk)
						BEGIN
							SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_stk
							SELECT @IsBaseUnit = IsBaseUnit, @Seq = Seq FROM Inventory.UoMConversion WHERE UomId = @UomId AND Id = @UnitId

							IF @IsBaseUnit = 1 
							BEGIN
								UPDATE Inventory.StockMutation SET BaseQty = Qty,BaseUnit = UnitId WHERE Id = @Id			
							END
							ELSE
							BEGIN
								UPDATE Inventory.StockMutation 
								SET BaseQty = Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
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
						IF OBJECT_ID('tempdb.dbo.#tmp_adj') IS NOT NULL
							DROP TABLE #tmp_adj
						IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
							DROP TABLE #tmp_old_stk
						IF OBJECT_ID('tempdb.dbo.#tmp_stk') IS NOT NULL
							DROP TABLE #tmp_stk
	
						-- Raise error
						EXEC dbo.sp_raiseerror
					END CATCH
					";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_adj_bak]
							@code varchar(17),
							@date date
						AS
						SET NOCOUNT ON
						BEGIN TRY

							SELECT d.*, h.WarehouseCode
							INTO #tmp_adj
							FROM Inventory.AdjustmentDetail d
							JOIN Inventory.AdjustmentHeader h on d.code = h.code
							WHERE d.Code = @code 

							-- Restore data warehouse qty from stock mutation
							SELECT *
							INTO #tmp_old_stk
							FROM Inventory.StockMutation 
							where RefCode1 = @code

							DECLARE @Id int
							DECLARE @UomId int
							DECLARE @UnitId int
							DECLARE @IsBaseUnit int
							DECLARE @BaseUnitQty int
							DECLARE @Seq int
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

							-- Delete stock mutation that doesn't have in adjustment item detail
							DELETE Inventory.StockMutation
							WHERE RefCode1 = @code
							AND Src = 'ADJ'
							AND NOT EXISTS (
								SELECT *
								FROM #tmp_adj
								WHERE Code = RefCode1
								AND Id = RefDetailId1
							)

							-- Update stock mutation that exists in adjustment item detail
							UPDATE sm
							SET sm.WarehouseCode = #tmp_adj.WarehouseCode,
								sm.[Date] = @date,
								sm.ItemId = #tmp_adj.ItemId,
								sm.UomId = #tmp_adj.UomId,
								sm.UnitId = #tmp_adj.UnitId,
								sm.Qty = #tmp_adj.QtyAdjust,
								sm.RefCode2 = NULL
							FROM Inventory.StockMutation sm, #tmp_adj
							WHERE sm.RefCode1 = #tmp_adj.Code
							AND sm.RefDetailId1 = #tmp_adj.Id
							AND sm.Src = 'ADJ'

							-- Insert stock mutation that doesn't have with adjustment item detail
							INSERT INTO Inventory.StockMutation
								SELECT 
									WarehouseCode, 
									@date, 
									ItemId, 
									UomId, 
									UnitId, 
									QtyAdjust,
									0,
									0,
									Code, 
									Id,
									NULL,
									'ADJ'
								FROM #tmp_adj adj
								WHERE NOT EXISTS (
									SELECT Id
									FROM Inventory.StockMutation sm
									WHERE sm.RefCode1 = adj.Code
									AND sm.RefDetailId1 = adj.Id
									AND sm.Src = 'ADJ'
								) And adj.QtyAdjust <> 0
							-- Drop temp tables
							DROP TABLE #tmp_adj
	
							SELECT *
							INTO #tmp_stk
							FROM Inventory.StockMutation WHERE RefCode1 = @code
	
							WHILE EXISTS(SELECT * FROM #tmp_stk)
							BEGIN
								SELECT TOP 1 @Id = Id, @UomId = UomId, @UnitId = UnitId,@ItemId = ItemId, @WarehouseCode = WarehouseCode FROM #tmp_stk
								SELECT @IsBaseUnit = IsBaseUnit, @Seq = Seq FROM Inventory.UoMConversion WHERE UomId = @UomId AND Id = @UnitId

								IF @IsBaseUnit = 1 
								BEGIN
									UPDATE Inventory.StockMutation SET BaseQty = Qty,BaseUnit = UnitId WHERE Id = @Id			
								END
								ELSE
								BEGIN
									UPDATE Inventory.StockMutation 
									SET BaseQty = Qty * (SELECT EXP(SUM(LOG(Conversion))) FROM Inventory.UoMConversion WHERE UomId = @UomId AND Seq <= @Seq),
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
							IF OBJECT_ID('tempdb.dbo.#tmp_adj') IS NOT NULL
								DROP TABLE #tmp_adj
							IF OBJECT_ID('tempdb.dbo.##tmp_old_stk') IS NOT NULL
								DROP TABLE #tmp_old_stk
							IF OBJECT_ID('tempdb.dbo.#tmp_stk') IS NOT NULL
								DROP TABLE #tmp_stk
	
							-- Raise error
							EXEC dbo.sp_raiseerror
						END CATCH";
            migrationBuilder.Sql(sql);
        }
    }
}
