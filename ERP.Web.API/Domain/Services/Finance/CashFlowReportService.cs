using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Entity.Finance;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Finance
{
    public class CashFlowReportService : ICashFlowReportService
    {
        private readonly TenantContext _ctx;
        public CashFlowReportService(TenantContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<CashFlowReportAll> GetData(int type, string dateFrom, string dateTo)
        {
            string sqlPart = "", sqlPart1 = "", sqlPart2 = "", sqlPart3 = "";

            var dtCoa = _ctx.VwCoas.Where(x => x.IsActive && x.TypeId == 1);
            var dtCoaT = dtCoa;

            dtCoa = dtCoa.Where(x => !dtCoaT.Select(t => t.ParentId).Contains(x.Id)).OrderBy(x => x.Code);

            if (dtCoa.Any())
            {
                int idx = 0;
                foreach (var item in dtCoa)
                {
                    idx += 1;

                    sqlPart += $@"
                    {(idx == 1 ? "WITH " : ",")}cte_cb_result_{idx} AS (
                        SELECT * FROM #TmpCbResult
                        WHERE CbCoa = '{item.Code}'
                    )";

                    sqlPart3 += $@"
                    LEFT JOIN cte_cb_result_{idx} TB{idx}
                        ON TB{idx}.CoaCode = TBA.Coa AND TB{idx}.Sort = TBA.Sort";


                    sqlPart1 += $@"
                    ,CASE WHEN TBA.Sort NOT IN (3,4,7,8,11,12,15,17,18)
                        THEN ISNULL(TB{idx}.AmountOc,0) ELSE NULL 
                    END AS Amount{idx}";


                }

                sqlPart1 += ",TBA.AmountOc AS AmountAll";

                for (int i = dtCoa.Count() + 1; i <= 30; i++)
                {
                    sqlPart2 += $",0.00 AS Amount{i}";
                }
            }

            string sql = GetSource(null, dateFrom ?? "1900/01/01",
                dateTo ?? DateTime.Now.AddYears(10).ToString("yyyy/MM/dd"));

            if (type == 1)
            {
                sql += $@"
                {sqlPart}

                SELECT TBA.*
                {sqlPart1}
                {sqlPart2}
                FROM (
                    SELECT CoaCode AS Coa,Description,Sort,IsBold,TypeTranIO
                    ,SUM(AmountOc) AS AmountOc,SUM(AmountIdr) AS AmountIdr,NULL AS TransCode,NULL AS Notes
					FROM #TmpCbResult
					GROUP BY CoaCode,Description,Sort,IsBold,TypeTranIO
                ) TBA
                {sqlPart3}
                ORDER BY TBA.Sort

                DROP TABLE #TmpCbResult";
            }
            else
            {
                sql += $@"
                {sqlPart}

                SELECT TBA.*
                {sqlPart1}
                {sqlPart2}
                FROM (
                    SELECT CoaCode AS Coa,Description,Sort,IsBold,TypeTranIO
                    ,SUM(AmountOc) AS AmountOc,SUM(AmountIdr) AS AmountIdr,TransCode,Notes
					FROM #TmpCbResult
					GROUP BY CoaCode,Description,Sort,IsBold,TypeTranIO,TransCode,Notes
                ) TBA
                {sqlPart3}
                ORDER BY TBA.Sort

                DROP TABLE #TmpCbResult";
            }

            return _ctx.CashFlowReportAlls.FromSqlRaw(sql).ToList();
        }

        public IEnumerable<CashFlowReportByCOA> GetDataCOA(int type, string coa, string dateFrom, string dateTo)
        {
            string sql = GetSource(coa, dateFrom ?? "1900/01/01", 
                dateTo ?? DateTime.Now.AddYears(10).ToString("yyyy/MM/dd"));

            if (type == 1)
            {
                sql += @"
                SELECT TBA.*
                FROM (
                    SELECT CoaCode AS Coa,Description,Sort,IsBold,TypeTranIO
                    ,SUM(AmountOc) AS AmountOc,SUM(AmountIdr) AS AmountIdr,NULL AS TransCode,NULL AS Notes
					FROM #TmpCbResult
					GROUP BY CoaCode,Description,Sort,IsBold,TypeTranIO
                ) TBA
                ORDER BY TBA.Sort
                
                DROP TABLE #TmpCbResult";
            }
            else
            {
                sql += @"
                SELECT TBA.*
                FROM (
                    SELECT CoaCode AS Coa,Description,Sort,IsBold,TypeTranIO
                    ,SUM(AmountOc) AS AmountOc,SUM(AmountIdr) AS AmountIdr,TransCode,Notes
					FROM #TmpCbResult
					GROUP BY CoaCode,Description,Sort,IsBold,TypeTranIO,TransCode,Notes
                ) TBA
                ORDER BY TBA.Sort
                
                DROP TABLE #TmpCbResult";
            }

            return _ctx.CashFlowReportByCOAs.FromSqlRaw(sql).ToList();
        }

        private static string GetSource(string coaCode, string dateFrom, string dateTo)
        {
            string whCbCoa = "";
            string whDateFrom = "", whDateFrom2 = "", whDateTo = "";

            if (!string.IsNullOrEmpty(coaCode))
                whCbCoa = $"AND CoaCode = '{coaCode.Replace("'", "''")}'";

            if (!string.IsNullOrEmpty(dateFrom))
            {
                dateFrom = dateFrom.Replace("'", "''");
                whDateFrom = $"AND DATEDIFF(DAY, [Date], '{dateFrom}') <= 0";
                whDateFrom2 = $"AND DATEDIFF(DAY, [Date], '{dateFrom}') > 0";
            }

            if (!string.IsNullOrEmpty(dateTo))
            {
                dateTo = dateTo.Replace("'", "''");
                whDateTo = $"AND DATEDIFF(DAY, [Date], '{dateTo}') >= 0";
            }


            string sql = $@"
                DECLARE @periodStart AS VARCHAR(6) = FORMAT(DATEADD(d,1,CONVERT(DATE,'{dateFrom}')),'yyyyMM'),
                    @periodEnd AS VARCHAR(6) = FORMAT(DATEADD(d,1,CONVERT(DATE,'{dateTo}')),'yyyyMM')
                ;WITH cte_cb_h_src AS (
                    SELECT DISTINCT Code,VouCode,[Date],Rate,CurrCode,CoaCode AS CbCoa
                    FROM Finance.GeneralCashBankHeader
                    WHERE Mark = 'A'
                    {whDateTo}
                    {whCbCoa}
                )
                ,cte_cb_all_src AS (
                    SELECT TBA.*,TBB.[Date],TBB.CbCoa
                    ,ISNULL(TBC.Name,'') AS CoaName
                    ,CASE WHEN TBB.CurrCode = 'IDR' THEN 1 ELSE 1 END AS RateBb
                    ,CASE WHEN TBB.CurrCode = 'IDR' THEN 1 ELSE 1 END AS RateEb
                    FROM (
                        SELECT Code,CoaCode,[Type],TransCode,Notes
                        ,CASE TypeAmount WHEN 'C' THEN 'I' WHEN 'D' THEN 'O' END AS TypeTranIO
                        ,CASE TypeAmount WHEN 'C' THEN Amount WHEN 'D' THEN -1 * Amount END AS AmountOc
                        FROM Finance.GeneralCashBankDetail TBA
                        --WHERE [Status] = 1
                    ) TBA
                    INNER JOIN cte_cb_h_src TBB
                        ON TBB.Code = TBA.Code
                    LEFT JOIN Accounting.COA TBC
                        ON TBC.Code = TBA.CoaCode
                        AND TBC.IsActive = 1
                )
                ,cte_cb_d_src AS (
                    SELECT
                    CbCoa,CoaCode,CoaName,TypeTranIO,[Type],AmountOc,AmountOc * RateEb AS AmountIdr,TransCode,Notes
                    FROM cte_cb_all_src
                    WHERE 1 = 1 {whDateFrom}
                )
                ,cte_cb_in_src AS (
                    SELECT
                    '' AS CbCoa,'' AS CoaCode,'Penerimaan' AS Description,4 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,NULL AS AmountOc,NULL AS AmountIdr,NULL AS TransCode,NULL AS Notes

                    UNION ALL
                    SELECT
                    CbCoa,CoaCode,'- ' + CoaName AS Description,5 AS Sort,'0' AS IsBold,TypeTranIO
                    ,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountIdr),0) AS AmountIdr,TransCode,Notes
                    FROM cte_cb_d_src
                    WHERE TypeTranIO = 'I' AND [Type] NOT IN ('ICBO','ICBI')
                    GROUP BY CbCoa,CoaCode,CoaName,TypeTranIO,TransCode,Notes

                    UNION ALL
					SELECT 
					'' AS CbCoa,'' AS CoaCode,'Sub Total Penerimaan' AS Description,6 AS Sort,'1' AS IsBold,'' AS TypeTranIO
					,0 AS AmountOc,0 AS AmountIdr,NULL AS TransCode,NULL AS Notes

                    UNION ALL
					SELECT
					CbCoa,'' AS CoaCode,'Sub Total Penerimaan' AS Description,6 AS Sort,'1' AS IsBold,'' AS TypeTranIO
					,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountIdr),0) AS AmountIdr,NULL AS TransCode,NULL AS Notes
					FROM cte_cb_d_src
					WHERE TypeTranIO = 'I' AND [Type] NOT IN ('ICBO','ICBI')
                    GROUP BY CbCoa

                    UNION ALL
                    SELECT
                    '' AS CbCoa,'' AS CoaCode,'' AS Description,7 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,NULL AS AmountOc,NULL AS AmountIdr,NULL AS TransCode,NULL AS Notes
                )
                ,cte_cb_out_src AS (
                    SELECT
                    '' AS CbCoa,'' AS CoaCode,'Pengeluaran' AS Description,8 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,NULL AS AmountOc,NULL AS AmountIdr,NULL AS TransCode,NULL AS Notes

                    UNION ALL
                    SELECT
                    CbCoa,CoaCode,'- ' + CoaName AS Description,9 AS Sort,'0' AS IsBold,TypeTranIO
                    ,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountIdr),0) AS AmountIdr,TransCode,Notes
                    FROM cte_cb_d_src
                    WHERE TypeTranIO = 'O' AND [Type] NOT IN ('ICBO','ICBI')
                    GROUP BY CbCoa,CoaCode,CoaName,TypeTranIO,TransCode,Notes

                    UNION ALL
					SELECT
                    '' AS CbCoa,'' AS CoaCode,'Sub Total Pengeluaran' AS Description,10 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,0 AS AmountOc,0 AS AmountIdr,NULL AS TransCode,NULL AS Notes

                    UNION ALL
                    SELECT
                    CbCoa,'' AS CoaCode,'Sub Total Pengeluaran' AS Description,10 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountIdr),0) AS AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM cte_cb_d_src
                    WHERE TypeTranIO = 'O' AND [Type] NOT IN ('ICBO','ICBI')
                    GROUP BY CbCoa

                    UNION ALL
                    SELECT
                    '' AS CbCoa,'' AS CoaCode,'' AS Description,11 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,NULL AS AmountOc,NULL AS AmountIdr,NULL AS TransCode,NULL AS Notes
                )
                ,cte_cb_icb_src AS (
                    SELECT
                    '' AS CbCoa,'' AS CoaCode,'Pindah Buku' AS Description,12 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,NULL AS AmountOc,NULL AS AmountIdr,NULL AS TransCode,NULL AS Notes

                    UNION ALL
                    SELECT
                    CbCoa,CoaCode,'- ' + CoaName AS Description,13 AS Sort,'0' AS IsBold,'ICB' AS TypeTranIO
                    ,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountIdr),0) AS AmountIdr,TransCode,Notes
                    FROM cte_cb_d_src
                    WHERE [Type] IN ('ICBO','ICBI')
                    GROUP BY CbCoa,CoaCode,CoaName,TransCode,Notes

                    UNION ALL
					SELECT
                    '' AS CbCoa,'' AS CoaCode,'Sub Total Pindah Buku' AS Description,14 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,0 AS AmountOc,0 AS AmountIdr,NULL AS TransCode,NULL AS Notes

                    UNION ALL
                    SELECT
                    CbCoa,'' AS CoaCode,'Sub Total Pindah Buku' AS Description,14 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountIdr),0) AS AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM cte_cb_d_src
                    WHERE [Type] IN ('ICBO','ICBI')
                    GROUP BY CbCoa

                    UNION ALL
                    SELECT
                    '' AS CbCoa,'' AS CoaCode,'' AS Description,15 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,NULL AS AmountOc,NULL AS AmountIdr,NULL AS TransCode,NULL AS Notes
                )
                ,cte_cb_beg_src AS (
                    SELECT
                    '' AS CbCoa,'' AS CoaCode,'Saldo Awal' AS Description,1 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,0 AS AmountOc,0 AS AmountIdr,NULL AS TransCode,NULL AS Notes
                    
                    UNION ALL
                    SELECT
                    CbCoa,'' AS CoaCode,'Saldo Awal' AS Description,1 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountOc * RateBb),0) AS AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM cte_cb_all_src
                    WHERE 1 = 1 {whDateFrom2}
                    GROUP BY CbCoa

                    UNION ALL
					SELECT
                    '' AS CbCoa,'' AS CoaCode,'Unrealized Gain Awal' AS Description,2 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,0 AS AmountOc,0 AS AmountIdr,NULL AS TransCode,NULL AS Notes

                    UNION ALL
                    SELECT
                    CbCoa,'' AS CoaCode,'Unrealized Gain Awal' AS Description,2 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,0.00 AS AmountOc,ISNULL(SUM(AmountOc * (RateEb - RateBb)), 0) AS AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM cte_cb_all_src
                    WHERE 1 = 1 {whDateFrom2}
                    GROUP BY CbCoa
                    
                    UNION ALL
                    SELECT
                    '' AS CbCoa,'' AS CoaCode,'' AS Description,3 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,NULL AS AmountOc,NULL AS AmountIdr,NULL AS TransCode,NULL AS Notes
                )
                ,cte_cb_end_src AS (
                    SELECT DISTINCT
                    '' AS CbCoa,'' AS CoaCode,'Saldo Akhir' AS Description,16 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,0 AS AmountOc,0 AS AmountIdr,NULL AS TransCode,NULL AS Notes

					UNION ALL
                    SELECT
                    CbCoa,'' AS CoaCode,'Saldo Akhir' AS Description,16 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountIdr),0) AS AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM (
                        SELECT CbCoa,SUM(AmountOc) AS AmountOc,SUM(AmountIdr) AS AmountIdr
                        FROM cte_cb_beg_src
                        WHERE cbCoa <> ''
                        GROUP BY CbCoa
                        UNION ALL
                        SELECT CbCoa,SUM(AmountOc) AS AmountOc,SUM(AmountIdr) AS AmountIdr
                        FROM cte_cb_d_src
                        GROUP BY CbCoa
                    ) TBA
                    GROUP BY CbCoa

                    UNION
                    SELECT
                    '' AS CbCoa,'' AS CoaCode,'' AS Description,17 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,NULL AS AmountOc,NULL AS AmountIdr,NULL AS TransCode,NULL AS Notes
                )
                ,cte_cb_sum_src AS (
                    SELECT '' AS CbCoa,'' AS CoaCode,'SUMMARY' AS Description,18 AS Sort,'1' AS IsBold,'' AS TypeTranIO
                    ,NULL AS AmountOc,NULL AS AmountIdr,NULL AS TransCode,NULL AS Notes

                    UNION ALL
                    SELECT CbCoa,'' AS CoaCode,'- ' + Description AS Description,19 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,AmountOc,AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM cte_cb_beg_src
                    WHERE Sort = 1

                    UNION ALL
                    SELECT CbCoa,'' AS CoaCode,'- ' + Description AS Description,20 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,AmountOc,AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM cte_cb_beg_src
                    WHERE Sort = 2

                    UNION ALL
                    SELECT CbCoa,'' AS CoaCode,'- Penerimaan' AS Description,21 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountIdr),0) AS AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM cte_cb_in_src
					WHERE Sort IN (4,5)
					GROUP BY CbCoa

                    UNION ALL
                    SELECT CbCoa,'' AS CoaCode,'- Pengeluaran' AS Description,22 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountIdr),0) AS AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM cte_cb_out_src
					WHERE Sort IN (8,9)
					GROUP BY CbCoa

                    UNION ALL
                    SELECT CbCoa,'' AS CoaCode,'- Pindah Buku' AS Description,23 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,ISNULL(SUM(AmountOc),0) AS AmountOc,ISNULL(SUM(AmountIdr),0) AS AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM cte_cb_icb_src
					WHERE Sort IN (12,13)
					GROUP BY CbCoa

                    UNION ALL
                    SELECT CbCoa,'' AS CoaCode,'- ' + Description AS Description,24 AS Sort,'0' AS IsBold,'' AS TypeTranIO
                    ,AmountOc,AmountIdr,NULL AS TransCode,NULL AS Notes
                    FROM cte_cb_end_src
                    WHERE Sort = 16
                )
                SELECT * INTO #TmpCbResult
                FROM (
                    SELECT * FROM cte_cb_beg_src
                    UNION ALL
                    SELECT * FROM cte_cb_in_src
                    UNION ALL
                    SELECT * FROM cte_cb_out_src 
                    UNION ALL
                    SELECT * FROM cte_cb_icb_src 
                    UNION ALL
                    SELECT * FROM cte_cb_end_src 
                    UNION ALL
                    SELECT * FROM cte_cb_sum_src 
                ) A;";

            return sql;
        }
    }
}
