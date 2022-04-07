using Microsoft.EntityFrameworkCore;
using ERP.Entity;
using ERP.Entity.MobileSales;
using ERP.Web.API.Domain.Interfaces.MobileSales;

namespace ERP.Web.API.Domain.Services.MobileSales;

public class MobileVisitPerformanceReportService : IMobileVisitPerformanceReportService
{
    private readonly TenantContext _db;

    public MobileVisitPerformanceReportService(TenantContext db)
    {
        _db = db;
    }
    public IEnumerable<MobileVisitPerformanceReport> GetData(string startDate, string endDate, int? salesId)
    {
        string wh = "";

        if (!string.IsNullOrEmpty(startDate))
            wh += $" AND DATEDIFF(DAY, [Date], '{startDate.Replace("'", "''")}') <= 0";

        if (!string.IsNullOrEmpty(endDate))
            wh += $" AND DATEDIFF(DAY, [Date], '{endDate.Replace("'", "''")}') >= 0";

        if (salesId.HasValue)
            wh += $" AND SalesmanId = {salesId}";

        string sql = @$"
                WITH cte_vo_src AS (
					SELECT vo.Code, vo.[Date], vo.SalesmanId, vo_c.CustCode, ISNULL(vo_c.Visited, 0) AS Visited
					FROM Sales.VisitOrder vo
					LEFT JOIN Sales.VisitOrderCustomer vo_c
						ON vo_c.Code = vo.Code
                    WHERE 1=1 {wh}
				)
				,cte_mvl_src AS (
					SELECT mvl.*,
						CASE WHEN mo.Code IS NOT NULL THEN 1 ELSE 0 END AS Invoiced
					FROM (
						SELECT Code, [Date], VisitOrderCode, SalesmanId, CustCode, Scheduled, Visited
						FROM MobileSales.MobileVisitLog
						WHERE Mark = 'APV' {wh}
					) mvl
					LEFT JOIN (
						SELECT Code, [Date], VisitLogCode, SalesOrderCode, CustCode, SalesBy
						FROM MobileSales.MobileOrderHeader
						WHERE Mark = 'APV'
					) mo
						ON mo.VisitLogCode = mvl.Code
				)
				,cte_union_src AS (
					SELECT Code, [Date], SalesmanId, 1 AS Scheduled
					FROM cte_vo_src
					UNION
					SELECT Code, [Date], SalesmanId, Scheduled
					FROM cte_mvl_src
					WHERE Scheduled = 0
				)
				,cte_final_src AS (
					SELECT cte_union.Code, cte_union.[Date], cte_union.SalesmanId,
						CONVERT(int, cte_union.Scheduled) AS Scheduled,
						CONVERT(int, cte_vo.Visited) AS Visited,
						ISNULL(cte_mvl_s.Invoiced, 0) AS ScheduledInvoiced,
						CASE WHEN cte_union.Scheduled = 1 THEN 0 ELSE 1 END AS Unscheduled,
						ISNULL(cte_mvl_u.Invoiced, 0) AS UnscheduledInvoiced,
                        e.Initial + ' - ' + e.FirstName AS SalesmanName
					FROM cte_union_src cte_union
					LEFT JOIN cte_vo_src cte_vo
						ON cte_vo.Code = cte_union.Code
						AND cte_union.Scheduled = 1
					LEFT JOIN cte_mvl_src cte_mvl_s
						ON cte_mvl_s.VisitOrderCode = cte_vo.Code
					LEFT JOIN cte_mvl_src cte_mvl_u
						ON cte_mvl_u.Code = cte_union.Code
						AND cte_union.Scheduled = 0
					LEFT JOIN General.Employee e
						ON e.Id = cte_union.SalesmanId
				)
				SELECT [Date], SalesmanId, SalesmanName,
					SUM(Scheduled) AS Scheduled,
					SUM(Visited) AS Visited,
					SUM(ScheduledInvoiced) AS ScheduledInvoiced,
					SUM(Unscheduled) AS Unscheduled,
					SUM(UnscheduledInvoiced) AS UnscheduledInvoiced
				FROM cte_final_src
				GROUP BY [Date], SalesmanId, SalesmanName";

        return _db.MobileVisitPerformanceReports.FromSqlRaw(sql);
    }
}