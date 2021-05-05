using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateTableAdjustmentAndOther : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdjustmentDetail",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    LineNo = table.Column<short>(type: "smallint", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    QtyOnHand = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    QtyAdjust = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BaseQtyOnHand = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    COGS = table.Column<decimal>(type: "decimal(19,6)", nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjustmentDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdjustmentHeader",
                schema: "Inventory",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Type = table.Column<short>(type: "smallint", nullable: false),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Notes = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Mark = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjustmentHeader", x => x.Code);
                });
            // alter view [Inventory].[vwItem]
            var sql = @"
                    ALTER VIEW [Inventory].[vwItem]
                    AS
	                    SELECT i.*,
		                    CASE i.TypeId
			                    WHEN 0 THEN 'Raw Material'
			                    WHEN 1 THEN 'Work In Process'
			                    WHEN 2 THEN 'Finished Good'
			                    WHEN 3 THEN 'Service'
			                    WHEN 4 THEN 'Consignment'
			                    WHEN 5 THEN 'Kits'
			                    WHEN 6 THEN 'Resell'
			                    ELSE '' END AS TypeName,
		                    c.[Name] AS CategoryName,
		                    uom.Initial AS UomInitial,
		                    uom_c_s.UnitEquivalent AS UomSellName,
		                    uom_c_b.UnitEquivalent AS UomBuyName,
		                    u.Initial AS UpdatedInitial,
		                    wq.WarehouseCode,
		                    ISNULL(wq.QtyOnHand, 0) As QtyOnHand
	                    FROM Inventory.Item i
	                    LEFT JOIN Inventory.ItemCategory c
		                    ON c.Id = i.CategoryId
	                    LEFT JOIN Inventory.UoM uom
		                    ON uom.Id = i.UomId
	                    LEFT JOIN Inventory.UoMConversion uom_c_s
		                    ON uom_c_s.Id = i.UomSellId
	                    LEFT JOIN Inventory.UoMConversion uom_c_b
		                    ON uom_c_b.Id = i.UomBuyId
	                    LEFT JOIN SystemManagement.[User] u
		                    ON u.Id = c.UpdatedBy
	                    LEFT JOIN Inventory.WarehouseQuantity wq
		                    ON wq.ItemId = i.Id";
            migrationBuilder.Sql(sql);

            // Create view [Inventory].[vwAdjustmentHeader]
            sql = @"
                CREATE View [Inventory].[vwAdjustmentHeader]
                as
                SELECT H.[Code]
                    ,H.[Date]
                    ,H.[Type]
                    ,H.[WarehouseCode]
	                ,W.Initial as Warehouse
                    ,H.[Notes]
                    ,H.[Mark]
                    ,H.[CreatedBy]
                    ,H.[CreatedDate]
                    ,H.[UpdatedBy]
                    ,H.[UpdatedDate]
	                ,CASE H.Mark
			                WHEN 'A' THEN 'Active'
			                WHEN 'V' THEN 'Void' END AS [Status]
                FROM [Inventory].[AdjustmentHeader] H
                JOIN [Inventory].Warehouse W ON H.WarehouseCode = W.Code";
            migrationBuilder.Sql(sql);

            // Create view [Inventory].[vwAdjustmentDetail]
            sql = @"
                CREATE view [Inventory].[vwAdjustmentDetail]
                as
                SELECT D.[Id]
                        ,D.[Code]
                        ,D.[LineNo]
                        ,D.[ItemId]
                        ,D.[UomId]
                        ,D.[UnitId]
                        ,D.[QtyOnHand]
	                    ,D.[QtyOnHand] + [QtyAdjust] as QtyOpname
	                    ,D.[QtyAdjust] as Different
                        ,D.[QtyAdjust] 
                        ,D.[BaseQtyOnHand]
                        ,D.[COGS]
                        ,D.[Notes]
	                    ,UC.[Initial] as UomInitial
	                    ,UC.[Description] as UomName
	                    ,I.[Name] as ItemName
                    FROM [Inventory].[AdjustmentDetail] D
                    JOIN [Inventory].UoM UC ON D.UomId = UC.Id
                    JOIN [Inventory].Item I ON D.ItemId = I.ID";
            migrationBuilder.Sql(sql);

			// Create view [dbo].[sp_update_stock_mutation_from_adj]
			sql = @"
                CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_adj]
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
						SET @BaseQty = (select BaseQty from Inventory.StockMutation where Id = @Id)
						UPDATE Inventory.WarehouseQuantity SET QtyOnhand = (QtyOnhand + @BaseQty) WHERE ItemId = @ItemId And WarehouseCode = @WarehouseCode
		
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdjustmentDetail",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "AdjustmentHeader",
                schema: "Inventory");

			// alter view [Inventory].[vwItem]
			var sql = @"
                    ALTER VIEW [Inventory].[vwItem]
                    AS
	                    SELECT i.*,
		                    CASE i.TypeId
			                    WHEN 0 THEN 'Raw Material'
			                    WHEN 1 THEN 'Work In Process'
			                    WHEN 2 THEN 'Finished Good'
			                    WHEN 3 THEN 'Service'
			                    WHEN 4 THEN 'Consignment'
			                    WHEN 5 THEN 'Kits'
			                    WHEN 6 THEN 'Resell'
			                    ELSE '' END AS TypeName,
		                    c.[Name] AS CategoryName,
		                    uom.Initial AS UomInitial,
		                    uom_c_s.UnitEquivalent AS UomSellName,
		                    uom_c_b.UnitEquivalent AS UomBuyName,
		                    u.Initial AS UpdatedInitial
	                    FROM Inventory.Item i
	                    LEFT JOIN Inventory.ItemCategory c
		                    ON c.Id = i.CategoryId
	                    LEFT JOIN Inventory.UoM uom
		                    ON uom.Id = i.UomId
	                    LEFT JOIN Inventory.UoMConversion uom_c_s
		                    ON uom_c_s.Id = i.UomSellId
	                    LEFT JOIN Inventory.UoMConversion uom_c_b
		                    ON uom_c_b.Id = i.UomBuyId
	                    LEFT JOIN SystemManagement.[User] u
		                    ON u.Id = c.UpdatedBy";
			migrationBuilder.Sql(sql);

			// Drop view [Inventory].[vwAdjustmentHeader]
			sql = @"DROP VIEW [Inventory].[vwAdjustmentHeader]";
			migrationBuilder.Sql(sql);

			// Drop view [Inventory].[vwAdjustmentDetail]
			sql = @"DROP VIEW [Inventory].[vwAdjustmentDetail]";
			migrationBuilder.Sql(sql);

			// Drop procedure [dbo].[sp_update_stock_mutation_from_adj]
			sql = @"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_adj]";
			migrationBuilder.Sql(sql);
		}
    }
}
