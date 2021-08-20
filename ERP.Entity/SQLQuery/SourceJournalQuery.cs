using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Entity.SQLQuery
{
    public class SourceJournalQuery
    {
        public static string BuildQuery(string dateFrom = null, string dateTo = null,
            string coaCode = null, string inCoaCode = null, string currCode = null,
            string rptType = null, string src = null, string inTypeCode = null)
        {
            string wh = "";

            if (!string.IsNullOrEmpty(dateFrom))
                wh += $" AND DATEDIFF(DAY, [Date], '{dateFrom.Replace("'", "''")}') <= 0";

            if (!string.IsNullOrEmpty(dateTo))
                wh += $" AND DATEDIFF(DAY, [Date], '{dateTo.Replace("'", "''")}') >= 0";

            if (!string.IsNullOrEmpty(currCode))
                wh += $" AND CurrCode = '{currCode.Replace("'", "''")}'";

            if (!string.IsNullOrEmpty(coaCode))
                wh += $" AND CoaCode = '{coaCode.Replace("'", "''")}'";

            if (!string.IsNullOrEmpty(inCoaCode))
                wh += $" AND CoaCode IN ({inCoaCode})";

            if (!string.IsNullOrEmpty(inTypeCode))
                wh += $" AND TypeCode IN ({inTypeCode})";

            if (!string.IsNullOrEmpty(src))
            {
                switch (src.ToUpper())
                {
                    case "CB":
                        wh += " AND SrcTrans IN ('CB', 'ADJ_UNREAL')";
                        break;
                    default:
                        wh += $" AND SrcTrans = '{src.Replace("'", "''")}'";
                        break;
                }
            }

            if (!string.IsNullOrEmpty(rptType))
                rptType = rptType.Replace("'", "''");

            string sql = $@"
                ;WITH cte_jur_src AS (
                    SELECT Code,[Date]
                    ,CASE WHEN '{rptType}' = '' THEN null ELSE [Type] END AS [Type]
                    ,CoaCode,TypeCode,Notes
                    ,RefCode1,RefCode2,RefCode3,RefCode4,[Group]
                    ,CurrCode,Period,CustomRate,SrcTrans
                    ,CASE WHEN [Type] = 'D' THEN Amount
                        ELSE -Amount END AS Amount
                    FROM Accounting.Journal WHERE SrcTrans IS NOT NULL
                    {wh}
                )
                ,cte_jur_src_group AS (
                    SELECT Code,[Date]
                    ,[Type],CoaCode,TypeCode,Notes
                    ,RefCode1,RefCode2,RefCode3,RefCode4,[Group]
                    ,CurrCode,Period,CustomRate,SrcTrans
                    ,SUM(Amount) AS amount
                    FROM cte_jur_src
                    GROUP BY Code,[Date]
                    ,[Type],CoaCode,TypeCode,Notes
                    ,RefCode1,RefCode2,RefCode3,RefCode4,[Group]
                    ,CurrCode,Period,CustomRate,SrcTrans
                )
                ,cte_jur_src_join AS (
                    SELECT TBA.*
                    ,TBB.Name AS coaName
                    ,CASE WHEN TBA.CurrCode = 'IDR' THEN 1
                        WHEN TBA.Period = 'CUSTOM' THEN CustomRate END AS rate
                    FROM cte_jur_src_group TBA
                    LEFT JOIN Accounting.COA TBB ON TBB.Code = TBA.CoaCode
                )
                ,cte_jur_src_calc AS (
                    SELECT Code,[Date]
                    ,CoaCode,TypeCode,Notes
                    ,RefCode1,RefCode2,RefCode3,RefCode4,[Group]
                    ,CurrCode,Period,SrcTrans,coaName,rate
                    ,CASE WHEN amount > 0 THEN 'D'
                        ELSE 'C' END AS [type]
                    ,CASE WHEN amount > 0 THEN amount
                        ELSE -amount END AS amount
                    ,CASE WHEN amount > 0 THEN amount
                        ELSE 0 END AS debetOC
                    ,CASE WHEN amount < 0 THEN -amount
                        ELSE 0 END AS creditOC
                    ,CASE WHEN amount > 0 THEN 1
                        ELSE 2 END AS sort_dc
                    FROM cte_jur_src_join
                )
                ,cte_jur_src_final AS (
                    SELECT *
                    FROM cte_jur_src_calc
                    WHERE amount > 0
                )";

            return sql;
        }
    }
}
