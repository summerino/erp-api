using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableAuthorization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Regex",
                schema: "SystemManagement",
                table: "Menu",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Action",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Action", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuAction",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuAction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenuAction",
                schema: "SystemManagement",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuAction", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Action_Name",
                schema: "SystemManagement",
                table: "Action",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuAction_MenuId_ActionId",
                schema: "SystemManagement",
                table: "MenuAction",
                columns: new[] { "MenuId", "ActionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuAction_RoleId_MenuId_ActionId",
                schema: "SystemManagement",
                table: "RoleMenuAction",
                columns: new[] { "RoleId", "MenuId", "ActionId" },
                unique: true);

            // Reorder column SystemManagement.Menu
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
CREATE TABLE SystemManagement.Tmp_Menu
	(
	Id int NOT NULL IDENTITY (1, 1),
	Name varchar(50) NOT NULL,
	ParentId int NULL,
	Deep int NOT NULL,
	Seq int NOT NULL,
	Link varchar(100) NULL,
	Icon varchar(50) NULL,
	Regex varchar(100) NULL,
	IsActive bit NOT NULL,
	CreatedBy int NOT NULL,
	CreatedDate datetime NOT NULL,
	UpdatedBy int NOT NULL,
	UpdatedDate datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE SystemManagement.Tmp_Menu SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT SystemManagement.Tmp_Menu ON
GO
IF EXISTS(SELECT * FROM SystemManagement.Menu)
	 EXEC('INSERT INTO SystemManagement.Tmp_Menu (Id, Name, ParentId, Deep, Seq, Link, Icon, Regex, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
		SELECT Id, Name, ParentId, Deep, Seq, Link, Icon, Regex, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate FROM SystemManagement.Menu WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT SystemManagement.Tmp_Menu OFF
GO
DROP TABLE SystemManagement.Menu
GO
EXECUTE sp_rename N'SystemManagement.Tmp_Menu', N'Menu', 'OBJECT' 
GO
ALTER TABLE SystemManagement.Menu ADD CONSTRAINT
	PK_Menu PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_update_stock_mutation_from_adj
			sql = @"ALTER PROCEDURE [dbo].[sp_update_stock_mutation_from_adj]
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
END CATCH";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Action",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "MenuAction",
                schema: "SystemManagement");

            migrationBuilder.DropTable(
                name: "RoleMenuAction",
                schema: "SystemManagement");

            migrationBuilder.DropColumn(
                name: "Regex",
                schema: "SystemManagement",
                table: "Menu");
        }
    }
}
