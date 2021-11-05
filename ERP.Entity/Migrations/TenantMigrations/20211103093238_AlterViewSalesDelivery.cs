using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewSalesDelivery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter view Sales.vwSalesDeliveryDetail
            var sql = @"ALTER VIEW [Sales].[vwSalesDeliveryDetail]
AS
	SELECT do_d.*,
		CASE WHEN do_h.SrcTrans = 1 THEN ISNULL(so_d.Qty, 0)
			 WHEN rtn_h.Type = 2 THEN ISNULL(rtn_d.Qty, 0)
			 WHEN rtn_h.Type = 3 THEN ISNULL(rtnx_d.Qty, 0)
			 ELSE 0 END AS OrderQty,
		CASE WHEN do_h.SrcTrans = 1 THEN ISNULL(so_d.Qty, 0) - ISNULL(so_d.QtyDlv, 0)
			 WHEN rtn_h.Type = 2 THEN ISNULL(rtn_d.Qty, 0) - ISNULL(rtn_d.QtyDlv, 0)
			 WHEN rtn_h.Type = 3 THEN ISNULL(rtnx_d.Qty, 0) - ISNULL(rtnx_d.QtyDlv, 0)
			 ELSE 0 END AS OutstandingQty,
		i.Initial AS ItemInitial,
		i.[Name] AS ItemName,
		i.UomSellId AS ItemUomSellId,
		uom_c_s.UnitEquivalent AS ItemUomSellName,
		i.SellPrice AS ItemSellPrice,
		uom.Initial AS UomInitial,
		uom_c.UnitEquivalent AS UnitName
	FROM Sales.SalesDeliveryDetail do_d
	LEFT JOIN Sales.SalesDeliveryHeader do_h
		ON do_h.Code = do_d.Code
	LEFT JOIN Sales.SalesReturnHeader rtn_h
		ON rtn_h.Code = do_h.TransCode
	LEFT JOIN Sales.SalesOrderDetail so_d
		ON so_d.Id = do_d.SODetailId
		AND do_h.SrcTrans = 1
	LEFT JOIN  Sales.SalesReturnDetail rtn_d
		ON rtn_d.Id = do_d.SODetailId
		AND do_h.SrcTrans = 2
	LEFT JOIN  Sales.SalesReturnDetailExchDiffItem rtnx_d
		ON rtnx_d.Id = do_d.SODetailId
		AND do_h.SrcTrans = 2
	LEFT JOIN Inventory.Item i
		ON i.Id = do_d.ItemId
	LEFT JOIN Inventory.UoMConversion uom_c_s
		ON uom_c_s.Id = i.UomSellId
	LEFT JOIN Inventory.UoM uom
		ON uom.Id = do_d.UomId
	LEFT JOIN Inventory.UoMConversion uom_c
		ON uom_c.Id = do_d.UnitId";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_update_sr_dlv_qty
			sql = @"ALTER PROCEDURE [dbo].[sp_update_sr_dlv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @returnQty decimal(18, 2), @dlvQty decimal(18, 2), @type int
	
	SELECT @type = [Type] FROM Sales.SalesReturnHeader rt_h WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
	IF (@type = 2)
	BEGIN
		-- Get sales return data
		SELECT Code, ItemId, UnitId, Qty
		INTO #tmp_rt
		FROM Sales.SalesReturnDetail rt_d
		WHERE EXISTS (
			SELECT Code
			FROM Sales.SalesReturnHeader rt_h
			WHERE Code = @code
			AND Mark NOT IN ('V', 'CLS')
			AND rt_h.Code = rt_d.Code
		)
	
		-- Return if sales return not exists
		IF NOT EXISTS (SELECT Code FROM #tmp_rt)
		BEGIN
			DROP TABLE #tmp_rt
			RETURN
		END
	
		-- Get sales delivery data
		SELECT ItemId, UnitId, SUM(Qty) AS QtyDlv
		INTO #tmp_do
		FROM Sales.SalesDeliveryDetail do_d
		WHERE EXISTS (
			SELECT Code
			FROM Sales.SalesDeliveryHeader do_h
			WHERE TransCode = @code
			AND Mark <> 'V'
			AND do_h.Code = do_d.Code
		)
		GROUP BY ItemId, UnitId

		-- Join all
		SELECT rt.Code, rt.ItemId, rt.UnitId, rt.Qty,
			ISNULL(do.QtyDlv, 0) AS QtyDlv
		INTO #tmp_all
		FROM #tmp_rt rt
		LEFT JOIN #tmp_do do
			ON do.ItemId = rt.ItemId
			AND do.UnitId = rt.UnitId

		-- Drop temp tables
		DROP TABLE #tmp_rt
		DROP TABLE #tmp_do

		-- Calculate sum
		SELECT @returnQty = SUM(Qty),
			@dlvQty = SUM(QtyDlv)
		FROM #tmp_all

		-- Update rt header mark
		UPDATE Sales.SalesReturnHeader
		SET Mark = (
			CASE WHEN @dlvQty = 0 THEN 'A'
				WHEN @dlvQty >= @returnQty THEN 'CMP'
				ELSE 'PS' END
		)
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')

		-- Update rt detail delivery qty item
		UPDATE rt_d
		SET rt_d.QtyDlv = #tmp_all.QtyDlv
		FROM Sales.SalesReturnDetail rt_d, #tmp_all
		WHERE rt_d.Code = #tmp_all.Code
		AND rt_d.ItemId = #tmp_all.ItemId
		AND rt_d.UnitId = #tmp_all.UnitId

		-- Drop temp table
		DROP TABLE #tmp_all
	END
	ELSE IF (@type = 3)
	BEGIN
		-- Get sales return data
		SELECT Code, ItemId, UnitId, Qty
		INTO #tmp_rtx
		FROM Sales.SalesReturnDetailExchDiffItem rt_d
		WHERE EXISTS (
			SELECT Code
			FROM Sales.SalesReturnHeader rt_h
			WHERE Code = @code
			AND Mark NOT IN ('V', 'CLS')
			AND rt_h.Code = rt_d.Code
		)
	
		-- Return if sales return not exists
		IF NOT EXISTS (SELECT Code FROM #tmp_rtx)
		BEGIN
			DROP TABLE #tmp_rtx
			RETURN
		END
	
		-- Get sales delivery data
		SELECT ItemId, UnitId, SUM(Qty) AS QtyDlv
		INTO #tmp_dox
		FROM Sales.SalesDeliveryDetail do_d
		WHERE EXISTS (
			SELECT Code
			FROM Sales.SalesDeliveryHeader do_h
			WHERE TransCode = @code
			AND Mark <> 'V'
			AND do_h.Code = do_d.Code
		)
		GROUP BY ItemId, UnitId

		-- Join all
		SELECT rtx.Code, rtx.ItemId, rtx.UnitId, rtx.Qty,
			ISNULL(dox.QtyDlv, 0) AS QtyDlv
		INTO #tmp_allx
		FROM #tmp_rtx rtx
		LEFT JOIN #tmp_dox dox
			ON dox.ItemId = rtx.ItemId
			AND dox.UnitId = rtx.UnitId

		-- Drop temp tables
		DROP TABLE #tmp_rtx
		DROP TABLE #tmp_dox

		-- Calculate sum
		SELECT @returnQty = SUM(Qty),
			@dlvQty = SUM(QtyDlv)
		FROM #tmp_allx

		-- Update rt header mark
		UPDATE Sales.SalesReturnHeader
		SET Mark = (
			CASE WHEN @dlvQty = 0 THEN 'A'
				WHEN @dlvQty >= @returnQty THEN 'CMP'
				ELSE 'PS' END
		)
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')

		-- Update rt detail delivery qty item
		UPDATE rt_d
		SET rt_d.QtyDlv = #tmp_allx.QtyDlv
		FROM Sales.SalesReturnDetailExchDiffItem rt_d, #tmp_allx
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

        }
    }
}
