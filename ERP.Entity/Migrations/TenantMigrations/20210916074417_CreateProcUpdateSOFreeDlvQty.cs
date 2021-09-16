using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcUpdateSOFreeDlvQty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Create procedure dbo.sp_update_so_free_dlv_qty
			var sql = @"CREATE PROCEDURE [dbo].[sp_update_so_free_dlv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @orderQty decimal(18, 2), @dlvQty decimal(18, 2)

	-- Get sales order data
	SELECT Code, ItemId, UnitId, Qty
	INTO #tmp_so
	FROM Sales.SalesOrderDetailFreeGood so_d
	WHERE EXISTS (
		SELECT Code
		FROM Sales.SalesOrderHeader so_h
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
		AND so_h.Code = so_d.Code
	)
	
	-- Return if sales order not exists
	IF NOT EXISTS (SELECT Code FROM #tmp_so)
	BEGIN
		DROP TABLE #tmp_so
		RETURN
	END
	
	-- Get sales delivery data
	SELECT ItemId, UnitId, SUM(Qty) AS QtyDlv
	INTO #tmp_do
	FROM Sales.SalesDeliveryDetailFreeGood do_d
	WHERE EXISTS (
		SELECT Code
		FROM Sales.SalesDeliveryHeader do_h
		WHERE TransCode = @code
		AND Mark <> 'V'
		AND do_h.Code = do_d.Code
	)
	GROUP BY ItemId, UnitId

	-- Join all
	SELECT so.Code, so.ItemId, so.UnitId, so.Qty,
		ISNULL(do.QtyDlv, 0) AS QtyDlv
	INTO #tmp_all
	FROM #tmp_so so
	LEFT JOIN #tmp_do do
		ON do.ItemId = so.ItemId
		AND do.UnitId = so.UnitId

	-- Drop temp tables
	DROP TABLE #tmp_so
	DROP TABLE #tmp_do

	-- Calculate sum
	SELECT @orderQty = SUM(Qty),
		@dlvQty = SUM(QtyDlv)
	FROM #tmp_all

	-- Update SO header mark
	UPDATE Sales.SalesOrderHeader
	SET Mark = (
		CASE WHEN @dlvQty = 0 THEN 'A'
			WHEN @dlvQty >= @orderQty THEN 'CMP'
			ELSE 'PS' END
	)
	WHERE Code = @code
	AND Mark NOT IN ('V', 'CLS')

	-- Update SO detail delivery qty item
	UPDATE so_d
	SET so_d.QtyClosed = #tmp_all.QtyDlv
	FROM Sales.SalesOrderDetailFreeGood so_d, #tmp_all
	WHERE so_d.Code = #tmp_all.Code
	AND so_d.ItemId = #tmp_all.ItemId
	AND so_d.UnitId = #tmp_all.UnitId

	-- Drop temp table
	DROP TABLE #tmp_all

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_so') IS NOT NULL
		DROP TABLE #tmp_so
	IF OBJECT_ID('tempdb.dbo.#tmp_do') IS NOT NULL
		DROP TABLE #tmp_do
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

	-- Raise error
	EXEC dbo.sp_raiseerror
END CATCH";
            migrationBuilder.Sql(sql);
        }

		protected override void Down(MigrationBuilder migrationBuilder)
        {
			// Drop procedure dbo.sp_update_so_free_dlv_qty
            var sql = @"DROP PROCEDURE [dbo].[sp_update_so_free_dlv_qty]";
            migrationBuilder.Sql(sql);
        }
	}
}
