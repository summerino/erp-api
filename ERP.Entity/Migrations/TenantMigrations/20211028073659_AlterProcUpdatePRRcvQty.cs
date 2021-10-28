using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterProcUpdatePRRcvQty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter procedure dbo.sp_update_pr_rcv_qty
            var sql = @"ALTER PROCEDURE [dbo].[sp_update_pr_rcv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @rtnQty decimal(18, 2), @rcvQty decimal(18, 2), @type int
	
	SELECT @type = [Type] FROM Purchasing.PurchaseReturnHeader rt_h WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')

	-- Get purchase return data
	IF (@type = 2)
	BEGIN
		SELECT Code, ItemId, UnitId, Qty
		INTO #tmp_rt
		FROM Purchasing.PurchaseReturnDetail rt_d
		WHERE EXISTS (
			SELECT Code
			FROM Purchasing.PurchaseReturnHeader rt_h
			WHERE Code = @code
			AND Mark NOT IN ('V', 'CLS')
			AND rt_h.Code = rt_d.Code
		)
	
		-- Return if purchase return not exists
		IF NOT EXISTS (SELECT Code FROM #tmp_rt)
		BEGIN
			DROP TABLE #tmp_rt
			RETURN
		END
	
		-- Get purchase receive data
		SELECT ItemId, UnitId, SUM(Qty) AS QtyRcv
		INTO #tmp_pr
		FROM Purchasing.PurchaseReceiveDetail pr_d
		WHERE EXISTS (
			SELECT Code
			FROM Purchasing.PurchaseReceiveHeader pr_h
			WHERE TransCode = @code
			AND Mark <> 'V'
			AND pr_h.Code = pr_d.Code
		)
		AND [Type] = 0
		GROUP BY ItemId, UnitId

		-- Join all
		SELECT rt.Code, rt.ItemId, rt.UnitId, rt.Qty,
			ISNULL(pr.QtyRcv, 0) AS QtyRcv
		INTO #tmp_all
		FROM #tmp_rt rt
		LEFT JOIN #tmp_pr pr
			ON pr.ItemId = rt.ItemId
			AND pr.UnitId = rt.UnitId

		-- Drop temp tables
		DROP TABLE #tmp_rt
		DROP TABLE #tmp_pr

		-- Calculate sum
		SELECT @rtnQty = SUM(Qty),
			@rcvQty = SUM(QtyRcv)
		FROM #tmp_all

		-- Update rt header mark
		UPDATE Purchasing.PurchaseReturnHeader
		SET Mark = (
			CASE WHEN @rcvQty = 0 THEN 'A'
				WHEN @rcvQty >= @rtnQty THEN 'CMP'
				ELSE 'PR' END
		)
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')

		-- Update rt detail receive qty item
		UPDATE rt_d
		SET rt_d.QtyRcv = #tmp_all.QtyRcv
		FROM Purchasing.PurchaseReturnDetail rt_d, #tmp_all
		WHERE rt_d.Code = #tmp_all.Code
		AND rt_d.ItemId = #tmp_all.ItemId
		AND rt_d.UnitId = #tmp_all.UnitId

		-- Drop temp table
		DROP TABLE #tmp_all
	END
	ELSE IF (@type = 3)
	BEGIN
		SELECT Code, ItemId, UnitId, Qty
		INTO #tmp_rtx
		FROM Purchasing.PurchaseReturnDetailExchDiffItem rt_d
		WHERE EXISTS (
			SELECT Code
			FROM Purchasing.PurchaseReturnHeader rt_h
			WHERE Code = @code
			AND Mark NOT IN ('V', 'CLS')
			AND rt_h.Code = rt_d.Code
		)
	
		-- Return if purchase return not exists
		IF NOT EXISTS (SELECT Code FROM #tmp_rtx)
		BEGIN
			DROP TABLE #tmp_rtx
			RETURN
		END
	
		-- Get purchase receive data
		SELECT ItemId, UnitId, SUM(Qty) AS QtyRcv
		INTO #tmp_prx
		FROM Purchasing.PurchaseReceiveDetail pr_d
		WHERE EXISTS (
			SELECT Code
			FROM Purchasing.PurchaseReceiveHeader pr_h
			WHERE TransCode = @code
			AND Mark <> 'V'
			AND pr_h.Code = pr_d.Code
		)
		AND [Type] = 0
		GROUP BY ItemId, UnitId

		-- Join all
		SELECT rtx.Code, rtx.ItemId, rtx.UnitId, rtx.Qty,
			ISNULL(prx.QtyRcv, 0) AS QtyRcv
		INTO #tmp_allx
		FROM #tmp_rtx rtx
		LEFT JOIN #tmp_prx prx
			ON prx.ItemId = rtx.ItemId
			AND prx.UnitId = rtx.UnitId

		-- Drop temp tables
		DROP TABLE #tmp_rtx
		DROP TABLE #tmp_prx

		-- Calculate sum
		SELECT @rtnQty = SUM(Qty),
			@rcvQty = SUM(QtyRcv)
		FROM #tmp_allx

		-- Update rt header mark
		UPDATE Purchasing.PurchaseReturnHeader
		SET Mark = (
			CASE WHEN @rcvQty = 0 THEN 'A'
				WHEN @rcvQty >= @rtnQty THEN 'CMP'
				ELSE 'PR' END
		)
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')

		-- Update rt detail receive qty item
		UPDATE rt_d
		SET rt_d.QtyRcv = #tmp_allx.QtyRcv
		FROM Purchasing.PurchaseReturnDetailExchDiffItem rt_d, #tmp_allx
		WHERE rt_d.Code = #tmp_allx.Code
		AND rt_d.ItemId = #tmp_allx.ItemId
		AND rt_d.UnitId = #tmp_allx.UnitId

		-- Drop temp table
		DROP TABLE #tmp_allx
	END
	

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_rt') IS NOT NULL
		DROP TABLE #tmp_rt
	IF OBJECT_ID('tempdb.dbo.#tmp_pr') IS NOT NULL
		DROP TABLE #tmp_pr
	IF OBJECT_ID('tempdb.dbo.#tmp_all') IS NOT NULL
		DROP TABLE #tmp_all

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
