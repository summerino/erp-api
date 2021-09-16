using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ERP.Entity;
using ERP.Entity.Accounting;
using ERP.Entity.SQLQuery;
using ERP.Web.API.Domain.Interfaces.Accounting;

namespace ERP.Web.API.Domain.Services.Accounting
{
    public class IncomeStatementReportService : IIncomeStatementReportService
    {
        private readonly TenantContext _db;

        public IncomeStatementReportService(TenantContext db)
        {
            _db = db;
        }

        public IEnumerable<IncomeStatementResult> GetIncomeStatementLists(string periodType, string rptBy,
            string rptDet, string dateTo)
        {
            string parDateTo = "", nowPeriod = "", prevPeriod = "";
            string whEndYear = "";

            string tmpTableName = DateTime.Now.ToString("yyyyMMdd_hhmmss_fff");

            string fmtType = rptBy.Substring(1, 1);
            rptBy = rptBy[..1];

            if (!string.IsNullOrEmpty(dateTo))
            {
                var date = DateTime.Parse(dateTo);
                date = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

                parDateTo = date.ToString("yyyy-MM-dd");
                nowPeriod = date.ToString("yyyyMM");
                prevPeriod = date.AddMonths(-1).ToString("yyyyMM");
                if (periodType is "Y" or "A")
                    prevPeriod = date.AddYears(-1).ToString("yyyyMM");

                whEndYear = $"WHERE Code <> 'ENDYEAR-{nowPeriod[..4]}'";
            }

            string sqlAmount = $@"
                ,SUM(CASE WHEN isPeriod = '{nowPeriod}' THEN (isPm * isAmountIdr) ELSE 0 END) AS isNowAmountIdr
                ,SUM(CASE WHEN isPeriod = '{prevPeriod}' THEN (isPm * isAmountIdr) ELSE 0 END) AS isPrevAmountIdr
                ,CONVERT(decimal, 0) AS isNowYearAmountIdr,CONVERT(decimal, 0) AS isPrevYearAmountIdr";

            if (periodType == "Y")
            {
                sqlAmount = $@"
                ,SUM(CASE WHEN isPercent = 1
                        THEN CASE WHEN isPeriod <> '{nowPeriod[..4]}99'
                                THEN 0 ELSE (isPm * isAmountIdr) END
                        ELSE CASE WHEN LEFT(isPeriod,4) = '{nowPeriod[..4]}' AND isPeriod <= '{nowPeriod}'
                                THEN (isPm * isAmountIdr) ELSE 0 END
                END) AS isNowAmountIdr
                ,SUM(CASE WHEN isPercent = 1
                        THEN CASE WHEN isPeriod <> '{prevPeriod[..4]}99'
                                THEN 0 ELSE (isPm * isAmountIdr) END
                        ELSE CASE WHEN LEFT(isPeriod,4) = '{prevPeriod[..4]}' AND isPeriod <= '{prevPeriod}' AND isSource <> 'END_YEAR'
                                THEN (isPm * isAmountIdr) ELSE 0 END
                END) AS isPrevAmountIdr
                ,CONVERT(decimal, 0) AS isNowYearAmountIdr,CONVERT(decimal, 0) AS isPrevYearAmountIdr";
            }
            else if (periodType == "A")
            {
                sqlAmount = $@"
                ,SUM(CASE WHEN isPeriod = '{nowPeriod}' THEN (isPm * isAmountIdr) ELSE 0 END) AS isNowAmountIdr
                ,SUM(CASE WHEN isPeriod = '{prevPeriod}' AND isSource <> 'END_YEAR' THEN (isPm * isAmountIdr) ELSE 0 END) AS isPrevAmountIdr
                ,SUM(CASE WHEN isPercent = 1
                        THEN CASE WHEN isPeriod <> '{nowPeriod[..4]}99'
                                THEN 0 ELSE (isPm * isAmountIdr) END
                        ELSE CASE WHEN LEFT(isPeriod,4) = '{nowPeriod[..4]}' AND isPeriod <= '{nowPeriod}'
                                THEN (isPm * isAmountIdr) ELSE 0 END
                END) AS isNowYearAmountIdr
                ,SUM(CASE WHEN isPercent = 1
                        THEN CASE WHEN isPeriod <> '{prevPeriod[..4]}99'
                                THEN 0 ELSE (isPm * isAmountIdr) END
                        ELSE CASE WHEN LEFT(isPeriod,4) = '{prevPeriod[..4]}' AND isPeriod <= '{prevPeriod}' AND isSource <> 'END_YEAR'
                                THEN (isPm * isAmountIdr) ELSE 0 END
                END) AS isPrevYearAmountIdr";
            }

            var maxDeep = _db.IncomeStatementFormats.Where(x => x.Category == fmtType)
                            .Max(x => x.Deep);

            string sqlDeep = "";
            for (int i = 1; i <= maxDeep; i++)
            {
                sqlDeep += $@"
                ,cte_is_deep_src_{i} AS (
                    SELECT * FROM cte_is_deep_src_{i - 1}
                    UNION ALL
                    SELECT TBA.Code AS isCode,TBA.Name AS isName
                    ,TBA.ParentCode AS isParent,TBA.PercentOf AS isPercentOf,TBA.PercentFrom AS isPercentFrom
                    ,TBA.Position AS isPosition,TBA.Deep AS isDeep,TBA.Sort AS isSort,TBA.SubTotalSort AS isSubTotalSort
                    ,CASE WHEN ISNULL(TBB.isShowed,0) = 1 THEN 0 ELSE TBA.Detail END AS isDetail
                    ,TBA.Hidden AS isHidden,TBA.Bold AS isBold
                    ,1 AS isShowed
                    ,2 AS isOutdent
                    ,ISNULL(TBB.isShowed,0) AS isExpanded
                    ,ISNULL(TBB.isShowed,0) AS isHasChild
                    ,TBA.isPercent
                    ,ISNULL(TBB.isPeriod,'') AS isPeriod,ISNULL(TBB.isSource,'') AS isSource
                    ,SUM(ISNULL(TBB.isAmountIdr,0)) AS isAmountIdr
                    FROM (
                        SELECT *
                        FROM #tmpIsFormat_{tmpTableName}
                        WHERE SubTotalSort = 0 AND [Type] <> 'P'
                        AND Deep = {maxDeep + 1 - i}
                    ) TBA
                    LEFT JOIN cte_is_deep_src_{i - 1} TBB
                        ON TBB.isParent = TBA.Code
                    GROUP BY TBA.Code,TBA.Name,TBA.ParentCode,TBA.PercentOf,TBA.PercentFrom
                    ,TBA.Position,TBA.Deep,TBA.Sort,TBA.SubTotalSort
                    ,CASE WHEN ISNULL(TBB.isShowed,0) = 1 THEN 0 ELSE TBA.Detail END
                    ,TBA.Hidden,TBA.Bold,TBB.isShowed,TBA.isPercent
                    ,TBB.isPeriod,TBB.isSource
                )";
            }

            var maxSubTot = _db.IncomeStatementFormats.Where(x => x.Category == fmtType)
                            .Max(x => x.SubtotalSort);

            string sqlSubTot = "";
            for (int i = 1; i <= maxSubTot; i++)
            {
                sqlSubTot += $@"
                ,{(i == maxSubTot ? "cte_is_format_src_1" : "cte_is_subtot_src_" + i)} AS (
                    SELECT * FROM cte_is_subtot_src_{i - 1}
                    UNION ALL
                    SELECT TBA.Code AS isCode,TBA.Name AS isName
                    ,TBA.ParentCode AS isParent,TBA.PercentOf AS isPercentOf,TBA.PercentFrom AS isPercentFrom
                    ,TBA.Position AS isPosition,TBA.Deep AS isDeep,TBA.Sort AS isSort,TBA.SubTotalSort AS isSubTotalSort
                    ,TBA.Detail AS isDetail,TBA.Hidden AS isHidden,TBA.Bold AS isBold
                    ,1 AS isShowed
                    ,2 AS isOutdent
                    ,0 AS isExpanded
                    ,0 AS isHasChild
                    ,0 AS isPercent
                    ,TBC.isPeriod,TBC.isSource
                    ,SUM(ISNULL(TBC.isAmountIdr,0)) AS isAmountIdr
                    FROM (
                        SELECT *
                        FROM #tmpIsFormat_{tmpTableName}
                        WHERE SubTotalSort = {i}
                    ) TBA
                    INNER JOIN Accounting.IncomeStatementFormatSubtotal TBB
                        ON TBB.Code = TBA.Code
                    INNER JOIN cte_is_subtot_src_{i - 1} TBC
                        ON TBC.isCode = TBB.subCode
                    GROUP BY TBA.Code,TBA.Name,TBA.ParentCode,TBA.PercentOf,TBA.PercentFrom
                    ,TBA.Position,TBA.Deep,TBA.Sort,TBA.SubTotalSort,TBA.Detail,TBA.Hidden,TBA.Bold
                    ,TBC.isPeriod,TBC.isSource
                )";
            }

            string sqlSort = "";
            for (int i = 2; i <= maxDeep + 2; i++)
            {
                sqlSort += $@"
                UPDATE TBA SET
                isRptSort = ISNULL(TBB.isRptSort,'') + RIGHT('0000000' + CONVERT(VARCHAR,TBA.isSort), 7)
                FROM (
                    SELECT * FROM #tmpIsRpt_{tmpTableName}
                    WHERE isDeep = {i}
                ) TBA, (
                    SELECT * FROM #tmpIsRpt_{tmpTableName}
                    WHERE isDeep = {i - 1}
                ) TBB
                WHERE TBA.isParent = TBB.isCode";
            }

            string cteSource = "cte_jur_src_final";

            string sql = $@"
                {SourceJournalQuery.BuildQuery(null, parDateTo, null, null, null, null, null, null)}
		        SELECT CoaCode,coaName,FORMAT([Date],'yyyyMM') AS period
                ,CASE WHEN SrcTrans = 'END_YEAR' THEN SrcTrans ELSE '' END AS src
                ,SUM(ROUND(debetOc - creditOc,4)) AS amountIdr
                INTO #tmpJourSrc_{tmpTableName}
		        FROM {cteSource}
                {whEndYear}
                GROUP BY CoaCode,coaName,FORMAT([Date],'yyyyMM')
                ,CASE WHEN SrcTrans = 'END_YEAR' THEN SrcTrans ELSE '' END
	           
                SELECT *,CASE WHEN [Type] = 'P' THEN 1 ELSE 0 END AS isPercent
                INTO #tmpIsFormat_{tmpTableName}
                FROM Accounting.IncomeStatementFormat
                WHERE IsActive = 1 AND Category = '{fmtType}'

                ;WITH cte_is_deep_src_0 AS (
                    SELECT TBA.CoaCode AS isCode,TBA.coaName AS isName
                    ,TBB.isCode AS isParent
                    ,TBC.PercentOf AS isPercentOf,TBC.PercentFrom AS isPercentFrom
                    ,TBC.Position AS isPosition
                    ,ISNULL(TBC.Deep,0) + 1 AS isDeep
                    ,CONVERT(INT,TBA.CoaCode) AS isSort
                    ,0 AS isSubTotalSort
                    ,TBC.Detail AS isDetail
                    ,TBC.Hidden AS isHidden
                    ,0 AS isBold
                    ,{(rptBy == "1" ? "0" : "ByAccount")} AS isShowed
                    ,1 AS isOutdent
                    ,0 AS isExpanded
                    ,0 AS isHasChild
                    ,TBC.isPercent
                    ,TBA.period AS isPeriod,TBA.src AS isSource
                    ,SUM(TBA.amountIdr) AS isAmountIdr
                    FROM #tmpJourSrc_{tmpTableName} TBA
                    INNER JOIN (
                        SELECT Code
                        ,CASE WHEN 'S' = '{fmtType}' THEN isCode ELSE isDetCode END AS isCode
                        FROM Accounting.COA
                        WHERE isActive = 1
                    ) TBB
                        ON TBB.Code = TBA.CoaCode
                    LEFT JOIN #tmpIsFormat_{tmpTableName} TBC
                        ON TBC.Code = TBB.isCode
                    GROUP BY TBA.CoaCode,TBA.coaName,TBB.isCode
                    ,TBC.PercentOf,TBC.PercentFrom,TBC.Position
                    ,TBC.Deep,TBC.Detail,TBC.Hidden{(rptBy == "1" ? "" : ",TBC.ByAccount")},TBC.isPercent
                    ,TBA.period,TBA.src
                )
                {sqlDeep}
                SELECT * INTO #tmpIsSrc_{tmpTableName}
                FROM cte_is_deep_src_{maxDeep}

	            ;WITH cte_is_subtot_src_0 AS (
                    SELECT isCode,isName,isParent
                    ,isPercentOf,isPercentFrom,isPosition,isDeep,isSort,isSubTotalSort
                    ,isDetail,isHidden,isBold,isShowed,isOutdent,isExpanded,isHasChild,isPercent
                    ,isPeriod,isSource
                    ,SUM(ISNULL(isAmountIdr,0)) AS isAmountIdr
                    FROM #tmpIsSrc_{tmpTableName}
                    GROUP BY isCode,isName,isParent
                    ,isPercentOf,isPercentFrom,isPosition,isDeep,isSort,isSubTotalSort
                    ,isDetail,isHidden,isBold,isShowed,isOutdent,isExpanded,isHasChild,isPercent
                    ,isPeriod,isSource
                )
                {sqlSubTot}
                ,cte_is_haschild_src AS (
                    SELECT *
                    FROM cte_is_format_src_1
                    WHERE isHasChild = 1
                    UNION ALL
                    SELECT *
                    FROM cte_is_format_src_1
                )
                ,cte_is_format_src_2 AS (
                    SELECT TBA.isCode + 'Z' AS isCode
                    ,'Total ' + TBA.isName AS isName
                    ,TBA.isCode AS isParent
                    ,TBA.isPercentOf,TBA.isPercentFrom
                    ,TBA.isPosition
                    ,TBA.isDeep + 1 AS isDeep
                    ,ISNULL(TBB.isMaxSort,0) + 1 AS isSort
                    ,TBA.isSubTotalSort
                    ,0 AS isDetail
                    ,TBA.isHidden
                    ,1 AS isBold
                    ,TBA.isShowed
                    ,TBA.isOutdent - 1 AS isOutdent
                    ,TBA.isExpanded
                    ,0 AS isHasChild
                    ,0 AS isPercent
                    ,TBA.isPeriod,TBA.isSource
                    ,SUM(ISNULL(TBA.isAmountIdr,0)) AS isAmountIdr
                    FROM cte_is_haschild_src TBA
                    LEFT JOIN (
                        SELECT isParent,MAX(ISNULL(isSort,0)) AS isMaxSort
                        FROM cte_is_format_src_1
                        GROUP BY isParent
                    ) TBB
                        ON TBB.isParent = TBA.isCode
                    GROUP BY TBA.isCode,TBA.isName
                    ,TBA.isPercentOf,TBA.isPercentFrom,TBA.isPosition,TBA.isDeep
                    ,TBB.isMaxSort,TBA.isSubTotalSort,TBA.isHidden,TBA.isShowed,TBA.isOutdent
                    ,TBA.isExpanded,TBA.isPeriod,TBA.isSource
                )
                SELECT isCode,isName,isParent
                ,isPercentOf,isPercentFrom,isPosition,isDeep,isSort,isSubTotalSort
                ,isDetail,isHidden,isBold,isShowed,isOutdent,isExpanded,isHasChild,isPercent
                ,isPeriod,isSource
                ,CONVERT(VARCHAR,'') AS isRptSort
                ,CASE isPosition WHEN 'D' THEN 1 WHEN 'C' THEN -1 END AS isPm
                ,isAmountIdr
				INTO #tmpIsRpt_{tmpTableName}
                FROM cte_is_format_src_1
                WHERE isShowed = 1
                    UNION ALL
                SELECT isCode,isName,isParent
                ,isPercentOf,isPercentFrom,isPosition,isDeep,isSort,isSubTotalSort
                ,isDetail,isHidden,isBold,isShowed,isOutdent,isExpanded,isHasChild,isPercent
                ,isPeriod,isSource
                ,CONVERT(VARCHAR,'') AS isRptSort
                ,CASE isPosition WHEN 'D' THEN 1 WHEN 'C' THEN -1 END AS isPm
                ,isAmountIdr
                FROM cte_is_format_src_2
                OPTION (FORCE ORDER)

                INSERT INTO #tmpIsRpt_{tmpTableName}
                SELECT isCode,isName,isParent
                ,isPercentOf,isPercentFrom,isPosition,isDeep,isSort,isSubTotalSort
                ,isDetail,isHidden,isBold,isShowed,isOutdent,isExpanded,isHasChild,isPercent
                ,'{nowPeriod[..4]}99' AS isPeriod,isSource,isRptSort,isPm
                ,SUM(isAmountIdr) AS isAmountIdr
                FROM #tmpIsRpt_{tmpTableName}
                WHERE (isPeriod > '{nowPeriod[..4]}00' AND isPeriod <= '{nowPeriod}')
                GROUP BY isCode,isName,isParent
                ,isPercentOf,isPercentFrom,isPosition,isDeep,isSort,isSubTotalSort
                ,isDetail,isHidden,isBold,isShowed,isOutdent,isExpanded,isHasChild,isPercent
                ,isPeriod,isSource,isRptSort,isPm

                INSERT INTO #tmpIsRpt_{tmpTableName}
                SELECT isCode,isName,isParent
                ,isPercentOf,isPercentFrom,isPosition,isDeep,isSort,isSubTotalSort
                ,isDetail,isHidden,isBold,isShowed,isOutdent,isExpanded,isHasChild,isPercent
                ,'{prevPeriod[..4]}99' AS isPeriod,isSource,isRptSort,isPm
                ,SUM(isAmountIdr) AS isAmountIdr
                FROM #tmpIsRpt_{tmpTableName} 
                WHERE (isPeriod > '{prevPeriod[..4]}00' AND isPeriod <= '{prevPeriod}')
                AND isSource <> 'END_YEAR' {(periodType is "Y" or "A" ? "" : "AND 1 = 2")}
                GROUP BY isCode,isName,isParent
                ,isPercentOf,isPercentFrom,isPosition,isDeep,isSort,isSubTotalSort
                ,isDetail,isHidden,isBold,isShowed,isOutdent,isExpanded,isHasChild,isPercent
                ,isPeriod,isSource,isRptSort,isPm

                INSERT INTO #tmpIsRpt_{tmpTableName}
                SELECT TBA.Code,TBA.Name,TBA.ParentCode
                ,TBA.PercentOf,TBA.PercentFrom,TBA.Position,TBA.Deep,TBA.Sort,TBA.SubTotalSort
                ,TBA.Detail,TBA.hidden,TBA.Bold,1 AS isShowed,2 AS isOutdent,0 AS isExpanded,0 AS isHasChild,1 AS isPercent
                ,TBC.isPeriod,'' AS isSource,TBC.isRptSort
                ,CASE TBA.Position WHEN 'D' THEN 1 WHEN 'C' THEN -1 END AS isPm
                ,CASE WHEN ISNULL(TBC.isAmountIdr,0) = 0
                    THEN 100
                    ELSE -1 * ISNULL(TBB.isAmountIdr,0) / ISNULL(TBC.isAmountIdr,0) * 100
                END AS isAmountIdr
                FROM (
                    SELECT *
                    FROM #tmpIsFormat_{tmpTableName}
                    WHERE [Type] = 'P'
                ) TBA
                INNER JOIN (
                    SELECT isCode,isPeriod,isRptSort
				    ,SUM(ISNULL(isAmountIdr,0) * ISNULL(isPm,0)) AS isAmountIdr
                    FROM #tmpIsRpt_{tmpTableName} TBA
                    WHERE EXISTS (
                        SELECT PercentOf
                        FROM #tmpIsFormat_{tmpTableName} TBB
                        WHERE [Type] = 'P'
                        AND TBB.PercentOf = TBA.isCode
                    )
                    AND isSource <> 'END_YEAR'
                    GROUP BY isCode,isPeriod,isRptSort
                ) TBB
                    ON TBB.isCode = TBA.PercentOf
                INNER JOIN (
                    SELECT isCode,isPeriod,isRptSort
				    ,SUM(ISNULL(isAmountIdr,0) * ISNULL(isPm,0)) AS isAmountIdr
                    FROM #tmpIsRpt_{tmpTableName} TBA
                    WHERE EXISTS (
                        SELECT PercentFrom
                        FROM #tmpIsFormat_{tmpTableName} TBB
                        WHERE [Type] = 'P'
                        AND TBB.PercentFrom = TBA.isCode
                    )
                    AND isSource <> 'END_YEAR'
                    GROUP BY isCode,isPeriod,isRptSort
                ) TBC
                    ON TBC.isCode = TBA.PercentFrom
                    AND TBC.isPeriod = TBB.isPeriod

                UPDATE #tmpIsRpt_{tmpTableName} SET
                isRptSort = RIGHT('0000000' + CONVERT(VARCHAR,isSort), 7)
                FROM #tmpIsRpt_{tmpTableName}
                WHERE isDeep = 1
                {sqlSort}

                SELECT isCode,isName,isParent,isPosition,isDeep,isSort,isSubTotalSort
                ,isDetail,isHidden,isBold,isShowed,isOutdent,isExpanded,isHasChild,isPercent,isRptSort,isPm
                {sqlAmount}
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}99' THEN (isPm * isAmountIdr) ELSE 0 END) AS isYtdAmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}01' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth1AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}02' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth2AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}03' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth3AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}04' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth4AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}05' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth5AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}06' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth6AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}07' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth7AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}08' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth8AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}09' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth9AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}10' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth10AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}11' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth11AmountIdr
                ,SUM(CASE WHEN isPeriod = '{nowPeriod[..4]}12' THEN (isPm * isAmountIdr) ELSE 0 END) AS isMonth12AmountIdr
                INTO #tmpIsSum_{tmpTableName}
                FROM #tmpIsRpt_{tmpTableName}
                GROUP BY isCode,isName,isParent,isPosition,isDeep,isSort,isSubTotalSort
                ,isDetail,isHidden,isBold,isShowed,isOutdent,isExpanded,isHasChild,isPercent
                ,isRptSort,isPm

                SELECT isCode,CASE WHEN isHidden = 1 THEN '' ELSE isName END AS isName
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isNowAmountIdr END AS isNowAmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isPrevAmountIdr END AS isPrevAmountIdr

                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isYtdAmountIdr END AS isYtdAmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth1AmountIdr END AS isMonth1AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth2AmountIdr END AS isMonth2AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth3AmountIdr END AS isMonth3AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth4AmountIdr END AS isMonth4AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth5AmountIdr END AS isMonth5AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth6AmountIdr END AS isMonth6AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth7AmountIdr END AS isMonth7AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth8AmountIdr END AS isMonth8AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth9AmountIdr END AS isMonth9AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth10AmountIdr END AS isMonth10AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth11AmountIdr END AS isMonth11AmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isMonth12AmountIdr END AS isMonth12AmountIdr

                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isNowYearAmountIdr END AS isNowYearAmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null ELSE isPrevYearAmountIdr END AS isPrevYearAmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null
                    ELSE isNowAmountIdr - isPrevAmountIdr END AS isDiffAmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null
                    ELSE CASE WHEN isPrevAmountIdr = 0 THEN 0 ELSE (isNowAmountIdr - isPrevAmountIdr) / isPrevAmountIdr  * 100 END 
                END AS isDiffPercent
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null
                    ELSE isNowYearAmountIdr - isPrevYearAmountIdr END AS isDiffYearAmountIdr
                ,CASE WHEN isHidden = 1 OR isHasChild = 1 THEN null
                    ELSE CASE WHEN isPrevYearAmountIdr = 0 THEN 0 ELSE (isNowYearAmountIdr - isPrevYearAmountIdr) / isPrevYearAmountIdr  * 100 END 
                END AS isDiffYearPercent

                ,isBold,isDeep,isHasChild,isPm,isPercent
                FROM #tmpIsSum_{tmpTableName}
                ORDER BY isRptSort,isSort

                DROP TABLE #tmpIsSum_{tmpTableName}
                DROP TABLE #tmpIsRpt_{tmpTableName}
                DROP TABLE #tmpIsSrc_{tmpTableName}
                DROP TABLE #tmpIsFormat_{tmpTableName}
                DROP TABLE #tmpJourSrc_{tmpTableName}";

            return _db.IncomeStatementResults.FromSqlRaw(sql).ToList();
        }

        public IEnumerable<BsIsDetailResult> GetBsIsDetailLists(string typeFormat, string code, string PlusMinus,
            string dateFrom, string dateTo, string jourSrc)
        {
            string parDateTo = "";
            string whEndYear = "", whJourSrc = "";

            if (!string.IsNullOrEmpty(dateTo))
            {
                var date = DateTime.Parse(dateTo);
                date = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
                parDateTo = date.ToString("yyyy-MM-dd");
                var nowPeriod = date.ToString("yyyyMM");

                whEndYear = $"AND Code <> 'ENDYEAR{nowPeriod[..4]}'";
            }

            if (!string.IsNullOrEmpty(jourSrc))
            {
                jourSrc = jourSrc.Replace("'", "''");
                jourSrc = jourSrc.Replace(",", "','");
                whJourSrc = $"AND src IN ('{jourSrc}')";
            }

            typeFormat = typeFormat.Replace("'", "''");
            code = code.Replace("'", "''");

            var dataCoa = _db.NewCodes.FromSqlInterpolated($"exec sp_get_bsisdt_coa {typeFormat}, {code.Replace("Z", "")}").ToList();
            if (!dataCoa.Any())
                return Enumerable.Empty<BsIsDetailResult>();

            var sqlCoa = "";

            foreach (var item in dataCoa)
            {
                sqlCoa +=
                    $@"{(sqlCoa == "" ? "" : " UNION ")} 
                    SELECT {item.Value} AS Code";
            }

            string cteSource = "cte_jur_src_final";

            string sql = $@"
                {SourceJournalQuery.BuildQuery(dateFrom, parDateTo, null, sqlCoa, null, null, null, null)}
	            ,cte_jour_src AS (
		            SELECT CoaCode,coaName,FORMAT([Date],'yyyyMM') AS period
                    ,CASE WHEN SrcTrans = 'END_YEAR' THEN SrcTrans ELSE '' END AS SrcTrans
		            ,SUM(ROUND(debetOc - creditOc,4) * {PlusMinus.Replace("'", "''")}) AS amountOc
                    ,SUM(ROUND(debetOc - creditOc,4) * {PlusMinus.Replace("'", "''")}) AS amountIdr
		            FROM {cteSource}
                    WHERE 1 = 1 {whEndYear} {whJourSrc}
                    GROUP BY CoaCode,coaName,FORMAT([Date],'yyyyMM')
                    ,CASE WHEN SrcTrans = 'END_YEAR' THEN SrcTrans ELSE '' END
	            )
                SELECT CoaCode,coaName
                ,SUM(amountOc) AS amountOc
                ,SUM(amountIdr) AS amountIdr
                FROM cte_jour_src
                GROUP BY CoaCode,coaName
                ORDER BY CoaCode";

            return _db.BsIsDetailResults.FromSqlRaw(sql).ToList();
        }
    }
}
