using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP.Entity.Migrations.TenantMigrations
{
    public partial class CreateProcBSISDtCoa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create procedure dbo.sp_get_bsisdt_coa
            var sql = @"CREATE PROCEDURE [dbo].[sp_get_bsisdt_coa]
	@type varchar(3),
	@code varchar(6)
AS
BEGIN
	DECLARE @looping int = 0

	--IF @type = 'BS'
	--BEGIN
	--	SELECT Code,0 AS isCoa
	--	INTO #tmpSourceBs
	--	FROM Accounting.BalanceSheetFormat
	--	WHERE Code = @code
	--	UNION ALL
	--	SELECT Code,1 AS isCoa
	--	FROM Accounting.COA
	--	WHERE Code = @code

	--	SELECT *
	--	INTO #tmpResultBs
	--	FROM #tmpSourceBs

	--	SELECT @looping = ISNULL(COUNT(*),0)
	--	FROM #tmpSourceBs
	--	WHERE isCoa = 0

	--	WHILE @looping > 0
	--	BEGIN
	--		WITH cte_dtcode_bs AS (
	--			SELECT f.Code,0 AS isCoa
	--			FROM #tmpSourceBs src
	--			INNER JOIN Accounting.BalanceSheetFormat f
	--				ON f.ParentCode = src.Code
	--				AND f.IsActive = 1
	--		)
	--		,cte_dtcode_bsst AS (
	--			SELECT f.Code,0 AS isCoa
	--			FROM #tmpSourceBs src
	--			INNER JOIN Accounting.BalanceSheetFormatSubtotal f_st
	--				ON f_st.Code = src.Code
	--			INNER JOIN Accounting.BalanceSheetFormat f
	--				ON f.Code = f_st.SubCode
	--				AND f.IsActive = 1
	--		)
	--		,cte_dtcode_coa AS (
	--			SELECT c.Code,1 AS isCoa
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
			
	--		SELECT @looping = ISNULL(COUNT(*),0) FROM #tmpSourceBs WHERE isCoa = 0
	--	END

	--	SELECT DISTINCT Code AS Value
	--	FROM #tmpResultBs
	
	--	DROP TABLE #tmpSourceBs
	--	DROP TABLE #tmpResultBs
	--END

	--ELSE IF @type = 'ISS'
	IF @type = 'ISS'
	BEGIN
		SELECT Code,0 AS isCoa
		INTO #tmpSourceIs
		FROM Accounting.IncomeStatementFormat
		WHERE Code = @code
		AND Category = RIGHT(@type,1)
		UNION ALL
		SELECT Code,1 AS isCoa
		FROM Accounting.COA
		WHERE IsCode = @code

		SELECT *
		INTO #tmpResultIs
		FROM #tmpSourceIs

		SELECT @looping = ISNULL(COUNT(*),0)
		FROM #tmpSourceIs
		WHERE isCoa = 0

		WHILE @looping > 0
		BEGIN
			WITH cte_dtcode_is AS (
				SELECT f.Code,0 AS isCoa
				FROM #tmpSourceIs src
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.ParentCode = src.Code
					AND f.IsActive = 1
			)
			,cte_dtcode_isst AS (
				SELECT f.Code,0 AS isCoa
				FROM #tmpSourceIs src
				INNER JOIN Accounting.IncomeStatementFormatSubtotal f_st
					ON f_st.Code = src.Code
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.Code = f_st.SubCode
					AND f.IsActive = 1
			)
			,cte_dtcode_coa AS (
				SELECT src.Code,1 AS isCoa
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
			
			SELECT @looping = ISNULL(COUNT(*),0) FROM #tmpSourceIs WHERE isCoa = 0
		END

		SELECT DISTINCT Code AS Value
		FROM #tmpResultIs
	
		DROP TABLE #tmpSourceIs
		DROP TABLE #tmpResultIs
	END

	ELSE IF @type = 'ISD'
	BEGIN
		SELECT Code,0 AS isCoa
		INTO #tmpSourceIsd
		FROM Accounting.IncomeStatementFormat
		WHERE Code = @code
		AND Category = RIGHT(@type,1)
		UNION ALL
		SELECT Code,1 AS isCoa
		FROM Accounting.COA
		WHERE IsDetCode = @code

		SELECT *
		INTO #tmpResultIsd
		FROM #tmpSourceIsd

		SELECT @looping = ISNULL(COUNT(*),0)
		FROM #tmpSourceIsd
		WHERE isCoa = 0

		WHILE @looping > 0
		BEGIN
			WITH cte_dtcode_is AS (
				SELECT f.Code,0 AS isCoa
				FROM #tmpSourceIsd src
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.ParentCode = src.Code
					AND f.IsActive = 1
			)
			,cte_dtcode_isst AS (
				SELECT f.Code,0 AS isCoa
				FROM #tmpSourceIsd src
				INNER JOIN Accounting.IncomeStatementFormatSubtotal f_st
					ON f_st.Code = src.Code
				INNER JOIN Accounting.IncomeStatementFormat f
					ON f.Code = f_st.SubCode
					AND f.IsActive = 1
			)
			,cte_dtcode_coa AS (
				SELECT c.Code,1 AS isCoa
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
			
			SELECT @looping = ISNULL(COUNT(*),0) FROM #tmpSourceIsd WHERE isCoa = 0
		END

		SELECT DISTINCT Code AS Value
		FROM #tmpResultIsd
	
		DROP TABLE #tmpSourceIsd
		DROP TABLE #tmpResultIsd
	END
END";
            migrationBuilder.Sql(sql);

			// Create function dbo.udf_string_split
			sql = @"CREATE FUNCTION [dbo].[udf_string_split]
(
	@string nvarchar(max),
	@separator nvarchar(4000)
)
RETURNS @output TABLE ([value] varchar(max))
AS
BEGIN

	DECLARE @idx int = 1
	DECLARE @slice varchar(max)

	IF ( LEN(@string) < 1 OR @string IS NULL ) RETURN

    SET @string = @string + @separator

    WHILE @idx !=  0
    BEGIN
		SET @idx = CHARINDEX(@separator, @string)
        SET @slice = SUBSTRING(@string, 0, @idx)

        INSERT INTO @output ([value])
        VALUES (@slice)

        SET @string = SUBSTRING(@string, @idx + 1, LEN(@string))
		IF LEN(@string) = 0 BREAK
    END

    RETURN

END";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
		{
			// Drop procedure dbo.sp_get_bsisdt_coa
			var sql = @"DROP PROCEDURE [dbo].[sp_get_bsisdt_coa]";
            migrationBuilder.Sql(sql);

            // Drop function dbo.udf_string_split
            sql = @"DROP FUNCTION [dbo].[udf_string_split]";
            migrationBuilder.Sql(sql);
        }
	}
}
