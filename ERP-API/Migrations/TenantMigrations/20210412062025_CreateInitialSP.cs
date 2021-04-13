using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_API.Migrations.TenantMigrations
{
    public partial class CreateInitialSP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create store procedure sp_generate_autono
            var sql = @"CREATE PROCEDURE [dbo].[sp_generate_autono]
	@code varchar(20),
	@date date = null,
	@newAutoNo varchar(20) = null OUTPUT
AS
BEGIN TRANSACTION

	DECLARE @result varchar(30) = ''

	IF ISNULL(@code, '') != ''
	BEGIN
		
		DECLARE @format varchar(32), @digitFormat varchar(32), @digit int,
				@myNumber varchar(20),
				@lastRunNo int
				
		/* Get format */
		SELECT @format = [Value]
		FROM msSystemParameters
		WHERE Code = @code
		
		IF @format IS NOT NULL
		BEGIN
		
			IF @date IS NULL
				SET @date = GETDATE()

			/* Replace format for date */
			SET @format = REPLACE(@format, '{Y}', FORMAT(@date, 'yy'))
			SET @format = REPLACE(@format, '{M}', FORMAT(@date, 'MM'))

			SET @digitFormat = SUBSTRING(@format, PATINDEX('%{[0-9]}%', @format), 5)
			SET @digit = CONVERT(int, REPLACE(REPLACE(@digitFormat, '{', ''), '}', ''))
	
			/* Get sequence number */
			SELECT @lastRunNo = LastRunNo
			FROM msSequenceNumbers
			WHERE Code = @code
			AND [Format] = @format

			IF @lastRunNo IS NULL
			BEGIN
				SET @lastRunNo = 0
			END

			SET @lastRunNo = @lastRunNo + 1
			
			SET @myNumber = '00000000000' + CONVERT(varchar, @lastRunNo)
			
			/* Replace format for sequence number */
			SET @result = REPLACE(@format, @digitFormat, RIGHT(@myNumber, @digit))

			/* Update history in msSequenceNumbers */
			UPDATE msSequenceNumbers
			SET LastRunNo = @lastRunNo
			WHERE Code = @code
			AND [Format] = @format

			/* Insert history in msSequenceNumbers if no record affected */
			IF @@ROWCOUNT = 0
			BEGIN
				INSERT INTO msSequenceNumbers (Code, [Format], LastRunNo)
				VALUES (@code, @format, @lastRunNo)
			END
		END

		/* Set output parameter */
		SET @newAutoNo = @result

	END
	
	/* return */
	SELECT @result AS value
	
COMMIT TRANSACTION";
            migrationBuilder.Sql(sql);

			// Create store procedure sp_raiseerror
            sql = @"CREATE PROCEDURE [dbo].[sp_raiseerror]
AS  
	DECLARE @errorNumber INT = ERROR_NUMBER();
	DECLARE @errorLine INT = ERROR_LINE();
	DECLARE @errorMessage NVARCHAR(4000) = ERROR_MESSAGE();
	DECLARE @errorSeverity INT = ERROR_SEVERITY();
	DECLARE @errorState INT = ERROR_STATE();

	RAISERROR(@errorMessage, @errorSeverity, @errorState);";
            migrationBuilder.Sql(sql);

			// Create store procedure sp_update_po_rcv_qty
            sql = @"CREATE PROCEDURE [dbo].[sp_update_po_rcv_qty]
	@code varchar(17)
AS
BEGIN TRY

	DECLARE @orderQty decimal(18, 2), @rcvQty decimal(18, 2)

	-- Get purchase order data
	SELECT Code, ItemId, UnitId, Qty
	INTO #tmp_po
	FROM trPurchaseOrderDetails po_d
	WHERE EXISTS (
		SELECT Code
		FROM trPurchaseOrderHeaders po_h
		WHERE Code = @code
		AND Mark NOT IN ('V', 'CLS')
		AND po_h.Code = po_d.Code
	)
	AND [Type] = 0
	
	-- Return if purchase order not exists
	IF NOT EXISTS (SELECT Code FROM #tmp_po)
	BEGIN
		DROP TABLE #tmp_po
		RETURN
	END
	
	-- Get purchase receive data
	SELECT ItemId, UnitId, SUM(Qty) AS QtyRcv
	INTO #tmp_pr
	FROM trPurchaseReceiveDetails pr_d
	WHERE EXISTS (
		SELECT Code
		FROM trPurchaseReceiveHeaders pr_h
		WHERE POCode = @code
		AND Mark <> 'V'
		AND pr_h.Code = pr_d.Code
	)
	AND [Type] = 0
	GROUP BY ItemId, UnitId

	-- Return if purchase receive not exists
	IF NOT EXISTS (SELECT ItemId FROM #tmp_pr)
	BEGIN
		DROP TABLE #tmp_pr
		RETURN
	END

	-- Join all
	SELECT po.Code, po.ItemId, po.UnitId, po.Qty,
		ISNULL(pr.QtyRcv, 0) AS QtyRcv
	INTO #tmp_all
	FROM #tmp_po po
	LEFT JOIN #tmp_pr pr
		ON pr.ItemId = po.ItemId
		AND pr.UnitId = po.UnitId

	-- Drop temp tables
	DROP TABLE #tmp_po
	DROP TABLE #tmp_pr

	-- Calculate sum
	SELECT @orderQty = SUM(Qty),
		@rcvQty = SUM(QtyRcv)
	FROM #tmp_all

	-- Update PO header mark
	UPDATE trPurchaseOrderHeaders
	SET Mark = (CASE WHEN @rcvQty >= @orderQty THEN 'CMP' ELSE 'PR' END)
	WHERE Code = @code
	AND Mark NOT IN ('V', 'CLS')

	-- Update PO detail receive qty item
	UPDATE po_d
	SET po_d.QtyRcv = #tmp_all.QtyRcv
	FROM trPurchaseOrderDetails po_d, #tmp_all
	WHERE po_d.Code = #tmp_all.Code
	AND po_d.ItemId = #tmp_all.ItemId
	AND po_d.UnitId = #tmp_all.UnitId

	-- Drop temp table
	DROP TABLE #tmp_all

END TRY
BEGIN CATCH
	-- Drop temp tables
	IF OBJECT_ID('tempdb.dbo.#tmp_po') IS NOT NULL
		DROP TABLE #tmp_po
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
			// Drop store procedure sp_generate_autono
			migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[sp_generate_autono]");

			// Drop store procedure sp_raiseerror
			migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[sp_raiseerror]");

			// Drop store procedure sp_update_po_rcv_qty
			migrationBuilder.Sql(@"DROP PROCEDURE [dbo].[sp_update_po_rcv_qty]");
		}
    }
}
