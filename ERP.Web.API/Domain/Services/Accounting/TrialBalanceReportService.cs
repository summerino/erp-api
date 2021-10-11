using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.SQLQuery;
using ERP.Web.API.Domain.Interfaces.Accounting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Services.Accounting
{
    public class TrialBalanceReportService : ITrialBalanceReportService
    {
        private readonly TenantContext _db;

        public TrialBalanceReportService(TenantContext db)
        {
            _db = db;
        }
        public IEnumerable<TrialBalanceResult> GetTrialBalanceLists(string rptBy, string dateFrom, string dateTo, string currCode)
        {
			string parRptBy = "", parCurrCode = null;
			string whEndYear = "";

			if (!string.IsNullOrEmpty(rptBy))
				parRptBy = (rptBy == "1" ? null : rptBy);

			if (!string.IsNullOrEmpty(currCode))
			{
				parCurrCode = (currCode == "-1" ? null : currCode);
				currCode = currCode.Replace("'", "''");
			}

			if (!string.IsNullOrEmpty(dateTo))
				whEndYear = $"WHERE Code <> 'ENDYEAR-' + CAST(YEAR('{dateTo.Replace("'", "''")}') AS VARCHAR)";

			if (!string.IsNullOrEmpty(dateFrom))
				dateFrom = dateFrom.Replace("'", "''");

			string cteSource = "cte_jur_src_final";

			string sql = $@"
                {SourceJournalQuery.BuildQuery(dateTo: dateTo, currCode: parCurrCode, rptType: parRptBy)}
	            ,cte_src_1 AS (
		            SELECT CoaCode,coaName,
					CASE WHEN '{currCode}' = '' THEN '{currCode}' ELSE CurrCode END AS currCode
		            ,LEFT(CoaCode,3) + '-1' AS sort,'0' AS isBold
		            ,CASE WHEN DATEDIFF(d,[Date],'{dateFrom}') > 0 THEN 'BEGIN' ELSE 'CURRENT' END AS typeTime
		            ,debetOC,creditOC
		            FROM {cteSource}
                    {whEndYear}
	            )
	            ,cte_src_2 AS (
		            SELECT CoaCode,coaName,CurrCode
		            ,sort,isBold
		            ,SUM(CASE WHEN typeTime = 'BEGIN' THEN debetOC - creditOC ELSE 0 END) AS beginBalIdr
		            ,SUM(CASE WHEN typeTime = 'BEGIN' THEN 0 ELSE debetOC END) AS debetIdr
		            ,SUM(CASE WHEN typeTime = 'BEGIN' THEN 0 ELSE creditOC END) AS creditIdr
		            FROM cte_src_1
		            GROUP BY CoaCode,coaName,CurrCode,sort,isBold
	            )
	            ,cte_src_3 AS (
		            SELECT *
		            ,beginBalIdr + debetIdr - creditIdr AS endBalIdr
		            FROM cte_src_2
	            )
	            ,cte_src_4 AS (
		            SELECT '' AS coaCode, 'Sub Total' AS coaName, '' AS currCode
		            ,LEFT(CoaCode,3) + '-2' AS sort,'1' AS isBold
		            ,SUM(beginBalIdr) AS beginBalIdr
		            ,SUM(debetIdr) AS debetIdr
		            ,SUM(creditIdr) AS creditIdr
		            ,SUM(endBalIdr) AS endBalIdr
		            FROM cte_src_3
		            GROUP BY LEFT(CoaCode,3)
	            )
                ,cte_src_5 AS (
		            SELECT DISTINCT '' AS coaCode,'' AS coaName,'' AS currCode
		            ,LEFT(CoaCode,3) + '-3' AS sort,'0' AS isBold
		            ,null AS beginBalIdr
		            ,null AS debetIdr
		            ,null AS creditIdr
		            ,null AS endBalIdr
		            FROM cte_src_3
	            )
				,cte_src_6 AS (
		            SELECT '' AS coaCode,'Total' AS coaName,'' AS currCode
		            ,'999-4' AS sort,'1' AS isBold
		            ,SUM(beginBalIdr) AS beginBalIdr
		            ,SUM(debetIdr) AS debetIdr
		            ,SUM(creditIdr) AS creditIdr
		            ,SUM(endBalIdr) AS endBalIdr
		            FROM cte_src_3
	            )
	            SELECT A.*
	            FROM (
		            SELECT * FROM cte_src_3
		            UNION
		            SELECT * FROM cte_src_4
                    UNION
		            SELECT * FROM cte_src_5
					UNION
		            SELECT * FROM cte_src_6
	            ) A
	            ORDER BY sort,CoaCode,CurrCode";

			return _db.TrialBalanceResults.FromSqlRaw(sql).ToList();
		}
    }
}
