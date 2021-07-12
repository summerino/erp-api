using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateTableStockMutationAndSP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trStockMutations",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseCode = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefCode1 = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    RefDetailId1 = table.Column<long>(type: "bigint", nullable: false),
                    RefCode2 = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: true),
                    Src = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trStockMutations", x => x.Id);
                });

            // Create store procedure sp_update_stock_mutation_from_rcv
            var sql = @"CREATE PROCEDURE [dbo].[sp_update_stock_mutation_from_rcv]
	@code varchar(17),
	@date date,
	@poCode varchar(17)
AS
BEGIN TRY

	SELECT *
	INTO #tmp_pr
	FROM trPurchaseReceiveDetails
	WHERE Code = @code

	-- Delete stock mutation that doesn't have in purchase receive item detail
	DELETE trStockMutations
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
		sm.RefCode2 = CASE WHEN [Type] = 0 THEN @poCode ELSE NULL END
	FROM trStockMutations sm, #tmp_pr
	WHERE sm.RefCode1 = #tmp_pr.Code
	AND sm.RefDetailId1 = #tmp_pr.Id
	AND sm.Src = 'RCV'

	-- Insert stock mutation that doesn't have with purchase receive item detail
	INSERT INTO trStockMutations
		SELECT WarehouseCode, @date, ItemId, UomId, UnitId, Qty, Code, Id,
			CASE WHEN [Type] = 0 THEN @poCode ELSE NULL END,
			'RCV'
		FROM #tmp_pr pr
		WHERE NOT EXISTS (
			SELECT Id
			FROM trStockMutations sm
			WHERE sm.RefCode1 = pr.Code
			AND sm.RefDetailId1 = pr.Id
			AND sm.Src = 'RCV'
		)

	-- Drop temp tables
	DROP TABLE #tmp_pr
	
END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trStockMutations",
                schema: "dbo");

            // Drop store procedure sp_update_stock_mutation_from_rcv
            migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[sp_update_stock_mutation_from_rcv]");
		}
    }
}
