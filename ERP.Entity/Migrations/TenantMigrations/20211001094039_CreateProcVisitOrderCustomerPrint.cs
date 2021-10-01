using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcVisitOrderCustomerPrint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create procedure dbo.sp_get_vo_cust_print_data
            var sql = @"CREATE PROCEDURE [dbo].[sp_get_vo_cust_print_data]
	@code varchar(17)
AS
BEGIN

	SELECT vo.Code, vo.[Date], vo.Notes,
		vo_c.CustCode, c.Initial AS CustInitial, c.[Name] AS CustName,
		ca_d.Address1 AS CustAddress1, ca_d.ContactPerson AS CustContactPerson,
		e_s.Initial AS SalesInitial, e_s.FirstName AS SalesFirstName, e_s.LastName AS SalesLastName,
		e_c.Initial AS CreatedInitial
	FROM (
		SELECT *
		FROM Sales.VisitOrder
		WHERE Code = @code
		AND Mark <> 'V'
	) vo
	LEFT JOIN Sales.VisitOrderCustomer vo_c
		ON vo_c.Code = vo.Code
	LEFT JOIN General.Customer c
		ON c.Code = vo_c.CustCode
	LEFT JOIN General.CustomerAddress ca_d
		ON ca_d.Code = c.Code
		and ca_d.IsDefault = 1
	LEFT JOIN General.Employee e_s
		ON e_s.Id = vo.SalesmanId
	LEFT JOIN General.Employee e_c
		ON e_c.Id = vo.CreatedBy
	
END";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_get_bsisdt_coa
			sql = @"ALTER PROCEDURE [dbo].[sp_get_bsisdt_coa]
	@type varchar(3),
	@code varchar(6)
AS
BEGIN
	DECLARE @looping int = 0

	--IF @type = 'BS'
	--BEGIN
	--	SELECT Code, 0 AS isCoa
	--	INTO #tmpSourceBs
	--	FROM Accounting.BalanceSheetFormat
	--	WHERE Code = @code
	--	UNION ALL
	--	SELECT Code, 1 AS isCoa
	--	FROM Accounting.COA
	--	WHERE Code = @code

	--	SELECT *
	--	INTO #tmpResultBs
	--	FROM #tmpSourceBs

	--	SELECT @looping = ISNULL(COUNT(*), 0)
	--	FROM #tmpSourceBs
	--	WHERE isCoa = 0

	--	WHILE @looping > 0
	--	BEGIN
	--		WITH cte_dtcode_bs AS (
	--			SELECT f.Code, 0 AS isCoa
	--			FROM #tmpSourceBs src
	--			INNER JOIN Accounting.BalanceSheetFormat f
	--				ON f.ParentCode = src.Code
	--				AND f.IsActive = 1
	--		)
	--		,cte_dtcode_bsst AS (
	--			SELECT f.Code, 0 AS isCoa
	--			FROM #tmpSourceBs src
	--			INNER JOIN Accounting.BalanceSheetFormatSubtotal f_st
	--				ON f_st.Code = src.Code
	--			INNER JOIN Accounting.BalanceSheetFormat f
	--				ON f.Code = f_st.SubCode
	--				AND f.IsActive = 1
	--		)
	--		,cte_dtcode_coa AS (
	--			SELECT c.Code, 1 AS isCoa
	--			FROM #tmpSourceBs src
	--			INNER JOIN Accounting.COA c
	--				ON c.BsCode = src.Code
	--		)
	--		SELECT *
	--		INTO #tmpDetailBs
	--		FROM (
	--			SELECT * FROM cte_dtcode_bs
	--			UNION
	--			SELECT * FROM cte_dtcode_bsst
	--			UNION
	--			SELECT * FROM cte_dtcode_coa
	--			UNION
	--			SELECT * FROM #tmpResultBs
	--			WHERE isCoa = 1
	--		) TBA

	--		DELETE FROM #tmpResultBs

	--		INSERT INTO #tmpResultBs
	--			SELECT *
	--			FROM #tmpDetailBs
	--			WHERE Code NOT IN (SELECT DISTINCT Code FROM #tmpSourceBs WHERE isCoa = 0)
			
	--		DROP TABLE #tmpDetailBs

	--		DELETE FROM #tmpSourceBs

	--		INSERT INTO #tmpSourceBs
	--			SELECT * FROM #tmpResultBs
			
	--		SELECT @looping = ISNULL(COUNT(*), 0) FROM #tmpSourceBs WHERE isCoa = 0
	--	END

	--	SELECT DISTINCT Code AS Value
	--	FROM #tmpResultBs
	
	--	DROP TABLE #tmpSourceBs
	--	DROP TABLE #tmpResultBs
	--END

	--ELSE IF @type = 'ISS'
	IF @type = 'ISS'
	BEGIN
		SELECT Code, 0 AS isCoa
		INTO #tmpSourceIs
		FROM Accounting.IncomeStatementFormat
		WHERE Code = @code
		AND Category = RIGHT(@type, 1)
		UNION ALL
		SELECT Code, 1 AS isCoa
		FROM Accounting.COA
		WHERE Code = @code

		SELECT *
		INTO #tmpResultIs
		FROM #tmpSourceIs

		SELECT @looping = ISNULL(COUNT(*), 0)
		FROM #tmpSourceIs
		WHERE isCoa = 0

		WHILE @looping > 0
		BEGIN
			WITH cte_dtcode_is AS (
				SELECT f.Code, 0 AS isCoa
				FROM #tmpSourceIs src
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.ParentCode = src.Code
					AND f.IsActive = 1
			)
			,cte_dtcode_isst AS (
				SELECT f.Code, 0 AS isCoa
				FROM #tmpSourceIs src
				INNER JOIN Accounting.IncomeStatementFormatSubtotal f_st
					ON f_st.Code = src.Code
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.Code = f_st.SubCode
					AND f.IsActive = 1
			)
			,cte_dtcode_coa AS (
				SELECT src.Code, 1 AS isCoa
				FROM #tmpSourceIs src
				INNER JOIN Accounting.COA c
					ON c.IsCode = src.Code
			)
			SELECT *
			INTO #tmpDetailIs
			FROM (
				SELECT * FROM cte_dtcode_is
				UNION
				SELECT * FROM cte_dtcode_isst
				UNION
				SELECT * FROM cte_dtcode_coa
				UNION
				SELECT * FROM #tmpResultIs
				WHERE isCoa = 1
			) TBA

			DELETE FROM #tmpResultIs

			INSERT INTO #tmpResultIs
				SELECT *
				FROM #tmpDetailIs
				WHERE Code NOT IN (SELECT DISTINCT Code FROM #tmpSourceIs WHERE isCoa = 0)
			
			DROP TABLE #tmpDetailIs

			DELETE FROM #tmpSourceIs

			INSERT INTO #tmpSourceIs
				SELECT * FROM #tmpResultIs
			
			SELECT @looping = ISNULL(COUNT(*), 0) FROM #tmpSourceIs WHERE isCoa = 0
		END

		SELECT DISTINCT Code AS Value
		FROM #tmpResultIs
	
		DROP TABLE #tmpSourceIs
		DROP TABLE #tmpResultIs
	END

	ELSE IF @type = 'ISD'
	BEGIN
		SELECT Code, 0 AS isCoa
		INTO #tmpSourceIsd
		FROM Accounting.IncomeStatementFormat
		WHERE Code = @code
		AND Category = RIGHT(@type, 1)
		UNION ALL
		SELECT Code, 1 AS isCoa
		FROM Accounting.COA
		WHERE Code = @code

		SELECT *
		INTO #tmpResultIsd
		FROM #tmpSourceIsd

		SELECT @looping = ISNULL(COUNT(*), 0)
		FROM #tmpSourceIsd
		WHERE isCoa = 0

		WHILE @looping > 0
		BEGIN
			WITH cte_dtcode_is AS (
				SELECT f.Code, 0 AS isCoa
				FROM #tmpSourceIsd src
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.ParentCode = src.Code
					AND f.IsActive = 1
			)
			,cte_dtcode_isst AS (
				SELECT f.Code, 0 AS isCoa
				FROM #tmpSourceIsd src
				INNER JOIN Accounting.IncomeStatementFormatSubtotal f_st
					ON f_st.Code = src.Code
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.Code = f_st.SubCode
					AND f.IsActive = 1
			)
			,cte_dtcode_coa AS (
				SELECT c.Code, 1 AS isCoa
				FROM #tmpSourceIsd src
				INNER JOIN Accounting.COA c
					ON c.IsDetCode = src.Code
			)
			SELECT *
			INTO #tmpDetailIsd
			FROM (
				SELECT * FROM cte_dtcode_is
				UNION
				SELECT * FROM cte_dtcode_isst
				UNION
				SELECT * FROM cte_dtcode_coa
				UNION
				SELECT * FROM #tmpResultIsd
				WHERE isCoa = 1
			) TBA

			DELETE FROM #tmpResultIsd

			INSERT INTO #tmpResultIsd
				SELECT *
				FROM #tmpDetailIsd
				WHERE Code NOT IN (SELECT DISTINCT Code FROM #tmpSourceIsd WHERE isCoa = 0)
			
			DROP TABLE #tmpDetailIsd

			DELETE FROM #tmpSourceIsd

			INSERT INTO #tmpSourceIsd
				SELECT * FROM #tmpResultIsd
			
			SELECT @looping = ISNULL(COUNT(*), 0) FROM #tmpSourceIsd WHERE isCoa = 0
		END

		SELECT DISTINCT Code AS Value
		FROM #tmpResultIsd
	
		DROP TABLE #tmpSourceIsd
		DROP TABLE #tmpResultIsd
	END
END";
            migrationBuilder.Sql(sql);

			// Alter procedure dbo.sp_get_cb_print_data
			sql = @"ALTER PROCEDURE [dbo].[sp_get_cb_print_data]
	@code varchar(17)
AS
BEGIN

	SELECT cb_h.Code, cb_h.VouCode, cb_h.[Type], cb_h.[Date], cb_h.CurrCode, ABS(cb_h.Amount) AS Amount, cb_h.ChequeNo, cb_h.Notes,
		CONCAT(cb_h.CoaCode, ' - ', c_1.[Name]) AS FullCoaName,
		CASE c_1.CBType
			WHEN 'C' THEN
				CASE cb_h.[Type]
					WHEN 'D' THEN 'BUKTI PENERIMAAN KAS'
					WHEN 'C' THEN 'BUKTI PENGELUARAN KAS'
				END
			WHEN 'B' THEN
				CASE cb_h.[Type]
					WHEN 'D' THEN 'BUKTI PENERIMAAN BANK'
					WHEN 'C' THEN 'BUKTI PENGELUARAN BANK'
				END
		END AS TitleCaption,
		cb_d.TransCode AS DetailTransCode, cb_d.Notes AS DetailNotes,
		CASE cb_h.[Type]
			WHEN 'C' THEN
				CASE cb_d.TypeAmount
					WHEN 'D' THEN cb_d.Amount
					WHEN 'C' THEN -cb_d.Amount
				END
			ELSE
				CASE cb_d.TypeAmount
					WHEN 'D' THEN -cb_d.Amount
					WHEN 'C' THEN cb_d.Amount
				END
		END AS DetailAmount,
		CONCAT(cb_d.CoaCode, ' - ', c_2.[Name]) AS DetailFullCoaName,
		e.Initial AS CreatedInitial
	FROM (
		SELECT *
		FROM Finance.GeneralCashBankHeader
		WHERE Code = @code
		AND Mark <> 'V'
	) cb_h
	LEFT JOIN Finance.GeneralCashBankDetail cb_d
		ON cb_d.Code = cb_h.Code
	LEFT JOIN Accounting.COA c_1
		ON c_1.Code = cb_h.CoaCode
	LEFT JOIN Accounting.COA c_2
		ON c_2.Code = cb_d.CoaCode
	LEFT JOIN General.Employee e
		ON e.Id = cb_h.CreatedBy
	
END";
            migrationBuilder.Sql(sql);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop procedure dbo.sp_get_vo_cust_print_data
            var sql = @"DROP PROCEDURE [dbo].[sp_get_vo_cust_print_data]";
            migrationBuilder.Sql(sql);
        }
    }
}
