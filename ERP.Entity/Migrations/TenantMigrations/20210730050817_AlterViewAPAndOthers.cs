using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class AlterViewAPAndOthers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
                name: "Src",
                schema: "Finance",
                table: "GeneralCashBankDetail",
                type: "varchar(5)",
                unicode: false,
                maxLength: 5,
                nullable: true);

			// Alter view Finance.vwGeneralCashBankHeader
			var sql = @"ALTER VIEW [Finance].[vwGeneralCashBankHeader] as 
SELECT h.*, 
	c.[Name] AS CoaName, 
	u_c.Initial AS CreatedInitial,
    u_u.Initial AS UpdatedInitial,
    u_a.Initial AS ApprovedInitial,
	CASE h.[Type]
		WHEN 'C' THEN 'Kas Bank Keluar'
		WHEN 'D' THEN 'Kas Bank Masuk' END AS [TypeName],
    CASE h.Mark
        WHEN 'A' THEN 'Active'
        WHEN 'V' THEN 'Void'
        WHEN 'CMP' THEN 'Completed'
        WHEN 'CLS' THEN 'Closed' END AS [Status]
FROM Finance.GeneralCashBankHeader h
LEFT JOIN Accounting.COA c 
	ON c.Code = h.CoaCode
LEFT JOIN SystemManagement.[User] u_c
    ON u_c.Id = h.CreatedBy
LEFT JOIN SystemManagement.[User] u_u
    ON u_u.Id = h.UpdatedBy
LEFT JOIN SystemManagement.[User] u_a
    ON u_a.Id = h.ApprovedBy";
            migrationBuilder.Sql(sql);

            // Alter view Finance.vwAP
            sql = @"ALTER VIEW [Finance].[vwAP]
AS
	SELECT B.Code, B.SupCode, S.[Name] AS SupName, [Date], CurrCode, Rate,
		Amount, PaidAmount, Amount - PaidAmount AS Remaining, B.Notes, 'BB' AS Src
	FROM Accounting.BeginningBalanceAP B
	JOIN General.Supplier S
		ON B.SupCode = S.Code
	WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
	UNION ALL
	SELECT P.Code,P.SupCode, S.[Name] AS SupName, P.[Date], P.CurrCode, 1 AS Rate,
		Total AS Amount, PaidAmount, Total - PaidAmount AS Remaining, P.Notes, 'PI' AS Src
	FROM Purchasing.PurchaseInvoiceHeader P
	JOIN General.Supplier S
		ON P.SupCode = S.Code
	WHERE P.PaidAmount < P.Total AND P.Mark != 'V'";
            migrationBuilder.Sql(sql);

            // Alter view Finance.vwAR
            sql = @"ALTER VIEW [Finance].[vwAR]
AS
	SELECT B.Code, B.CustCode, C.[Name] AS CustName, [Date], CurrCode, Rate AS Rate,
		Amount, PaidAmount, Amount - PaidAmount AS Remaining, B.Notes, 'BB' AS Src
	FROM Accounting.BeginningBalanceAR B
	JOIN General.Customer C
		ON B.CustCode = C.Code
	WHERE B.PaidAmount < B.Amount AND B.IsActive = 1
	UNION ALL
	SELECT S.Code,S.CustCode, C.[Name] AS CustName, S.[Date], S.CurrCode, 1 AS Rate,
		Total AS Amount, PaidAmount, Total - PaidAmount AS Remaining, S.Notes, 'SI' AS Src
	FROM Sales.SalesInvoiceHeader S
	JOIN General.Customer C
		ON S.CustCode = C.Code
	WHERE S.PaidAmount < S.Total AND S.Mark != 'V'";
            migrationBuilder.Sql(sql);

			// Alter view Accounting.vwBeginningBalanceCreditMemo
			sql = @"ALTER VIEW [Accounting].[vwBeginningBalanceCreditMemo]
AS
	SELECT cm.*,
		cm.Amount - cm.Used AS Remaining,
		c.[Name] AS CustName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceCreditMemo cm
	LEFT JOIN General.Customer c
		ON cm.CustCode = c.Code
	LEFT JOIN SystemManagement.[User] cr
		ON cm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON cm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

			// Alter view Accounting.vwBeginningBalanceDebitMemo
			sql = @"ALTER VIEW [Accounting].[vwBeginningBalanceDebitMemo]
AS
	SELECT dm.*,
		dm.Amount - dm.Used AS Remaining,
		s.[Name] AS SupName,
		cr.Initial AS CreatedInitial,
		up.Initial AS UpdatedInitial
	FROM Accounting.BeginningBalanceDebitMemo dm
	LEFT JOIN General.Supplier s
		ON dm.SupCode = s.Code
	LEFT JOIN SystemManagement.[User] cr
		ON dm.CreatedBy = cr.Id
	LEFT JOIN SystemManagement.[User] up
		ON dm.UpdatedBy = up.Id";
            migrationBuilder.Sql(sql);

			// Create view Finance.vwOutstandingCreditMemo
			sql = @"CREATE VIEW [Finance].[vwOutstandingCreditMemo]
AS
	SELECT bb.Code, [Date], bb.CustCode, c.[Name] AS CustName, bb.[Type], bb.CurrCode, bb.Rate,
		bb.Amount, bb.Used, bb.Amount - bb.Used AS Remaining, bb.Notes, 'BB' AS Src
	FROM Accounting.BeginningBalanceCreditMemo bb
	JOIN General.Customer c
		ON bb.CustCode = c.Code
	WHERE bb.Used < bb.Amount AND bb.IsActive = 1
	UNION ALL
	SELECT Code, [Date], CustCode, CustName, SrcTrans, CurrCode, 1 AS Rate,
		Amount, Used, Amount - Used AS Remaining, Notes, 'CM' AS Src
	FROM Sales.vwCreditMemo
	WHERE Used < Amount AND Mark IN ('A', 'PU')";
            migrationBuilder.Sql(sql);

			// Create view Finance.vwOutstandingDebitMemo
			sql = @"CREATE VIEW [Finance].[vwOutstandingDebitMemo]
AS
	SELECT bb.Code, [Date], bb.SupCode, c.[Name] AS SupName, bb.[Type], bb.CurrCode, bb.Rate,
		bb.Amount, bb.Used, bb.Amount - bb.Used AS Remaining, bb.Notes, 'BB' AS Src
	FROM Accounting.BeginningBalanceDebitMemo bb
	JOIN General.Supplier c
		ON bb.SupCode = c.Code
	WHERE bb.Used < bb.Amount AND bb.IsActive = 1
	UNION ALL
	SELECT Code, [Date], SupCode, SupName, SrcTrans, CurrCode, 1 AS Rate,
		Amount, Used, Amount - Used AS Remaining, Notes, 'DM' AS Src
	FROM Purchasing.vwDebitMemo
	WHERE Used < Amount AND Mark IN ('A', 'PU')";
            migrationBuilder.Sql(sql);

			// Drop view Finance.vwPR
			sql = @"DROP VIEW [Finance].[vwPR]";
            migrationBuilder.Sql(sql);

			// Drop view Finance.vwSR
            sql = @"DROP VIEW [Finance].[vwSR]";
            migrationBuilder.Sql(sql);

			// Refresh view Finance.vwGeneralCashBankDetail
			sql = @"execute sp_refreshview 'Finance.vwGeneralCashBankDetail'";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_restore_cash_bank_transaction
			sql = @"ALTER PROCEDURE [dbo].[sp_restore_cash_bank_transaction]
	@code varchar(17)
AS
BEGIN

	SET NOCOUNT ON;

	SELECT d.Id, d.TransCode as TransCode, d.[Type], d.TransAmount, d.Src
	INTO #tmp_cb
	FROM Finance.GeneralCashBankHeader h
	JOIN Finance.GeneralCashBankDetail d ON h.Code = d.Code
	WHERE h.Code = @code

	-- Restore transaction
	DECLARE @id int
	DECLARE @type varchar(5), @transCode varchar(17), @src varchar(5)
	DECLARE @transAmount decimal(18, 2)
	
	DECLARE @idDetail int
	DECLARE @totalDetail Decimal(18, 2)
	DECLARE @totalHeader Decimal(18, 2)
	DECLARE @paidAmount Decimal(18, 2)
	DECLARE @proRate Decimal(18, 2)
	Declare @mark varchar(3)
	DECLARE @used Decimal(18, 2)

	WHILE EXISTS(SELECT * FROM #tmp_cb)
	BEGIN
		
		SELECT TOP 1 @id = Id, @type = [Type], @transCode = TransCode, @transAmount = TransAmount, @src = Src
		FROM #tmp_cb
		
		IF (@type = 'AR' AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceAR 
			SET PaidAmount = PaidAmount - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'AR')
		BEGIN

			SELECT @totalHeader = Total, @paidAmount = PaidAmount - @transAmount   
			FROM Sales.SalesInvoiceHeader 
			WHERE Code = @transCode
				
			SET @mark = 'A'
			IF (@paidAmount > 0) SET @mark = 'PP'
			
			UPDATE Sales.SalesInvoiceHeader SET PaidAmount = @paidAmount, Mark = @mark WHERE Code = @transCode

			-- Update detail
			SELECT Id, Total, DoCode
			INTO #tmp_si
			FROM Sales.SalesInvoiceDetail
			WHERE Code = @transCode

			DECLARE @doCode varchar(17)

			WHILE EXISTS(SELECT * FROM #tmp_si)
			BEGIN
				SELECT TOP 1 @idDetail = Id, @totalDetail = Total, @doCode = DoCode FROM #tmp_si
				SET @proRate = @paidAmount * @totalDetail / @totalHeader
					
				UPDATE Sales.SalesDeliveryHeader SET PaidAmount = @proRate WHERE Code = @doCode 
					
				DELETE FROM #tmp_si
			END

			DROP TABLE #tmp_si

		END
		ELSE IF (@type = 'AP' AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceAP 
			SET PaidAmount = PaidAmount - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'AP')
		BEGIN

			SELECT @totalHeader = Total, @paidAmount = PaidAmount - @transAmount   
			FROM Purchasing.PurchaseInvoiceHeader 
			WHERE Code = @transCode
				
			SET @mark = 'A'
			IF (@paidAmount > 0) SET @mark = 'PP'

			UPDATE Purchasing.PurchaseInvoiceHeader SET PaidAmount = @paidAmount, Mark = @mark WHERE Code = @transCode

			-- Update detail
			SELECT Id, Total, RcvCode
			INTO #tmp_pi
			FROM Purchasing.PurchaseInvoiceDetail 
			WHERE Code = @transCode
				
			DECLARE @rcvCode varchar(17)

			WHILE EXISTS(SELECT * FROM #tmp_pi)
			BEGIN
				SELECT TOP 1 @idDetail = Id, @totalDetail = Total, @rcvCode = RcvCode FROM #tmp_pi
				SET @proRate = @paidAmount * @totalDetail / @totalHeader
					
				UPDATE Purchasing.PurchaseReceiveHeader SET PaidAmount = @proRate WHERE Code = @rcvCode 
					
				DELETE FROM #tmp_pi
			END

			DROP TABLE #tmp_pi

		END
		ELSE IF (@type = 'DPC')
		BEGIN

			UPDATE Sales.CreditMemo
			SET Mark = 'PP'
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'DPS')
		BEGIN

			UPDATE Purchasing.DebitMemo
			SET Mark = 'PP'
			WHERE Code = @transCode;

		END
		ELSE IF ((@type = 'RDPC' OR @type = 'SR') AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceCreditMemo
			SET Used = Used - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'RDPC' OR @type = 'SR')
		BEGIN

			SET @used = ((SELECT Used FROM Sales.CreditMemo where Code = @transCode) - @transAmount)
			SET @mark = 'A'
			IF (@used > 0) SET @mark = 'PU'
			UPDATE Sales.CreditMemo SET Used = @used, Mark=@mark WHERE Code = @transCode

		END
		ELSE IF ((@type = 'RDPS' OR @type = 'PR') AND @src = 'BB')
		BEGIN

			UPDATE Accounting.BeginningBalanceDebitMemo
			SET Used = Used - @transAmount
			WHERE Code = @transCode;

		END
		ELSE IF (@type = 'RDPS' OR @type = 'PR')
		BEGIN

			SET @used = ((SELECT Used FROM Purchasing.DebitMemo where Code = @transCode) - @transAmount)
			SET @mark = 'A'
			IF (@used > 0) SET @mark = 'PU'
			UPDATE Purchasing.DebitMemo SET Used = @used, Mark = 'A' WHERE Code = @transCode

		END

		DELETE #tmp_cb WHERE Id = @id

	END

	DROP TABLE #tmp_cb

END";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
		{
            migrationBuilder.DropColumn(
                name: "Src",
                schema: "Finance",
                table: "GeneralCashBankDetail");

			// Drop view Finance.vwOutstandingCreditMemo
			var sql = @"DROP VIEW [Finance].[vwOutstandingCreditMemo]";
            migrationBuilder.Sql(sql);

			// Create view Finance.vwPR
			sql = @"CREATE VIEW [Finance].[vwPR]
AS
	SELECT B.Code, B.SupCode, S.[Name] AS SupName, [Date], CurrCode, Rate,
		Amount, Used, Amount - Used AS Remaining, B.Notes, 'BB' AS Src
	FROM Accounting.BeginningBalanceDebitMemo B
	JOIN General.Supplier S
		ON B.SupCode = S.Code
	WHERE B.[Type] = 2 AND B.Used < B.Amount AND B.IsActive = 1
	UNION ALL
	SELECT P.Code,P.SupCode, S.[Name] AS SupName, P.[Date],P.CurrCode, 1 AS Rate,
		Amount, Used, Amount - Used AS Remaining, P.Notes, 'DM' AS Src
	FROM Purchasing.vwDebitMemo P
	JOIN General.Supplier S
		ON P.SupCode = S.Code
	WHERE P.SrcTrans = 2 AND P.Used < P.Amount AND P.Mark IN ('A', 'PU')";
            migrationBuilder.Sql(sql);

            // Create view Finance.vwSR
            sql = @"CREATE VIEW [Finance].[vwSR]
AS
	SELECT B.Code, B.CustCode, C.[Name] AS CustName, [Date], CurrCode, Rate,
		Amount, Used, Amount - Used AS Remaining, B.Notes, 'BB' AS Src
	FROM Accounting.BeginningBalanceCreditMemo B
	JOIN General.Customer C
		ON B.CustCode = C.Code
	WHERE B.[Type] = 2 AND B.Used < B.Amount AND B.IsActive = 1
	UNION ALL
	SELECT S.Code,S.CustCode, C.[Name] AS CustName, S.[Date], S.CurrCode, 1 AS Rate,
		Amount, Used, Amount - Used AS Remaining, S.Notes, 'CM' AS Src
	FROM Sales.vwCreditMemo S
	JOIN General.Customer C
		ON S.CustCode = C.Code
	WHERE S.SrcTrans = 2 AND S.Used < S.Amount AND S.Mark IN ('A', 'PU')";
            migrationBuilder.Sql(sql);
		}
	}
}
