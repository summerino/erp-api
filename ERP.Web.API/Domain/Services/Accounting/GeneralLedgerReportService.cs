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
    public class GeneralLedgerReportService : IGeneralLedgerReportService
    {
        private readonly TenantContext _db;

        public GeneralLedgerReportService(TenantContext db)
        {
            _db = db;
        }

        public IEnumerable<GeneralLedgerResult> GetGeneralLedgerLists(string dateFrom, string dateTo, string coaFrom, string coaTo, string currCode, string sort, int? caller)
        {
            string sqlCoa = "";

            if (!(string.IsNullOrEmpty(coaFrom) && string.IsNullOrEmpty(coaTo)))
            {
                coaFrom = string.IsNullOrEmpty(coaFrom) ? coaTo : coaFrom;
                coaTo = string.IsNullOrEmpty(coaTo) ? coaFrom : coaTo;

                int acc1 = int.TryParse(coaFrom, out int tmp) ? tmp : 0;
                int acc2 = int.TryParse(coaTo, out tmp) ? tmp : 0;

                if (!(acc1 == 0 && acc2 == 0))
                {
                    if (acc1 > acc2)
                    {
                        tmp = acc1;
                        acc1 = acc2;
                        acc2 = tmp;
                    }
                    coaFrom = acc1.ToString();
                    coaTo = acc2.ToString();
                }

                sqlCoa = $@"
                        SELECT DISTINCT Code
                        FROM Accounting.COA
                        WHERE Code BETWEEN '{coaFrom.Replace("'", "''")}' AND '{coaTo.Replace("'", "''")}'";
            }

            string whEndYear = "";

            if (DateTime.TryParse(dateTo, out var dateTemp))
            {
                if (dateTemp.Month == 12 && dateTemp.Day == 31)
                    whEndYear = $"AND Code <> 'ENDYEAR-{dateTemp.Year}'";
            }

            if (!string.IsNullOrEmpty(dateFrom))
                dateFrom = dateFrom.Replace("'", "''");

            string cteSource = "cte_jur_src_final";
            string orderBy = (sort == "2" ? "Code,[Date],RefCode1" : "[Date],Code,RefCode1");

            string sql = $@"
                {SourceJournalQuery.BuildQuery(null, dateTo, null, sqlCoa, currCode, null, null, null)}
	            ,cte_begin_src AS (
		            SELECT '' AS Code,null AS [Date],'Saldo Awal' AS Notes
                    ,CoaCode,coaName,'' AS RefCode1,'' AS RefCode2,'' AS RefCode3,'' AS RefCode4
                    ,'' AS CurrCode,null AS rate
                    ,null AS debetOc,null AS creditOc,SUM(debetOc-creditOc) AS endBalOc
                    ,'0' AS sort,'1' AS isBold
                    FROM (
                        SELECT DISTINCT CoaCode,coaName,null AS debetOc,null AS creditOc
						FROM {cteSource}
						UNION ALL
						SELECT CoaCode,coaName,debetOc,creditOc
		                FROM {cteSource}
                        WHERE DATEDIFF(d,[Date],'{dateFrom}') > 0
                    ) TBA
                    GROUP BY CoaCode,coaName
                )
                ,cte_current_src AS (
                    SELECT Code,[Date],Notes
                    ,CoaCode,coaName
                    ,RefCode1,RefCode2,RefCode3,RefCode4
                    ,CurrCode,rate
                    ,debetOc,creditOc
                    ,null AS endBalOc
                    ,CASE WHEN LEFT(Code,8) <> 'ENDYEAR-' THEN '1' ELSE '2' END AS sort
                    ,'0' AS isBold
		            FROM {cteSource}
                    WHERE DATEDIFF(d,[Date],'{dateFrom}') <= 0
                    {whEndYear}
                )
                ,cte_end_src AS (
                    SELECT '' AS Code,null AS [Date],'Sub Total' AS Notes
                    ,CoaCode,coaName
                    ,'' AS RefCode1,'' AS RefCode2,'' AS RefCode3,'' AS RefCode4
                    ,'' AS CurrCode,null AS rate
                    ,SUM(ISNULL(debetOc,0)) AS debetOc
                    ,SUM(ISNULL(creditOc,0)) AS creditOc
                    ,SUM(ISNULL(endBalOc,0) + ISNULL(debetOc,0) - ISNULL(creditOc,0)) AS endBalOc
                    ,'3' AS sort,'1' AS isBold
		            FROM (
                        SELECT * FROM cte_begin_src
                        UNION ALL
                        SELECT * FROM cte_current_src
                    ) TBA
                    GROUP BY CoaCode,coaName
                )
                ,cte_union_src AS (
                    SELECT * FROM cte_begin_src
                    UNION ALL
                    SELECT * FROM cte_current_src
                    UNION ALL
                    SELECT * FROM cte_end_src
                )
                SELECT *
                FROM cte_union_src
                ORDER BY CoaCode,sort,{orderBy}";

            return _db.GeneralLedgerResults.FromSqlRaw(sql).ToList();
        }
    }
}
