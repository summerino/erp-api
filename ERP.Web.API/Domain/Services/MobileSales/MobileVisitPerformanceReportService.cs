using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using ERP.Web.API.Model.MobileSales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.MobileSales
{
    public class MobileVisitPerformanceReportService : IMobileVisitPerformanceReportService
    {
        private readonly TenantContext _db;
        public MobileVisitPerformanceReportService(TenantContext db)
        {
            _db = db;
        }
        public DataSourceResult GetData(int? salesId, string startDate, string endDate)
        {
            List<MobileVisitPerformanceReportRequest> data = new();
			var empData = _db.Employees.ToList();
			var vData = _db.MobileVisitPerformanceReports.FromSqlRaw(@"WITH cte_vo_src AS (
							SELECT vo.Code, vo.[Date], vo.SalesmanId, vo_c.CustCode, vo_c.Visited
							FROM Sales.VisitOrder vo
							LEFT JOIN Sales.VisitOrderCustomer vo_c
								ON vo_c.Code = vo.Code
						)
						,cte_mvl_src AS (
							SELECT mvl.*,
								CASE WHEN mo.Code IS NOT NULL THEN 1 ELSE 0 END AS Invoiced
							FROM (
								SELECT Code, [Date], VisitOrderCode, SalesmanId, CustCode, Scheduled, Visited
								FROM MobileSales.MobileVisitLog 
								WHERE Mark = 'APV'
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
								ISNULL(cte_mvl_u.Invoiced, 0) AS UnscheduledInvoiced
							FROM cte_union_src cte_union
							LEFT JOIN cte_vo_src cte_vo
								ON cte_vo.Code = cte_union.Code
								AND cte_union.Scheduled = 1
							LEFT JOIN cte_mvl_src cte_mvl_s
								ON cte_mvl_s.VisitOrderCode = cte_vo.Code
							LEFT JOIN cte_mvl_src cte_mvl_u
								ON cte_mvl_u.Code = cte_union.Code
								AND cte_union.Scheduled = 0
						)
						SELECT [Date], CONVERT(int, SalesmanId) AS SalesmanId,
							CONVERT(int, SUM(Scheduled)) AS Scheduled,
							CONVERT(int, SUM(Visited)) AS Visited,
							CONVERT(int, SUM(ScheduledInvoiced)) AS ScheduledInvoiced,
							CONVERT(int, SUM(Unscheduled)) AS Unscheduled,
							CONVERT(int, SUM(UnscheduledInvoiced)) AS UnscheduledInvoiced
						FROM cte_final_src
						GROUP BY [Date], SalesmanId
						").ToList();

			if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
			{
				vData = vData.Where(x => x.Date >= Convert.ToDateTime(startDate) && x.Date <= Convert.ToDateTime(endDate)).ToList();
			}
			else if (!string.IsNullOrEmpty(startDate))
			{
				vData = vData.Where(x => x.Date >= Convert.ToDateTime(startDate)).ToList();
			}
			else if (!string.IsNullOrEmpty(endDate))
			{
				vData = vData.Where(x => x.Date <= Convert.ToDateTime(endDate)).ToList();
			}

			if (salesId.HasValue)
			{
				vData = vData.Where(x => x.SalesmanId == salesId).ToList();
			}

            foreach (var item in vData)
            {
				data.Add(new MobileVisitPerformanceReportRequest
				{
					Date = item.Date,
					SalesmanId = item.SalesmanId,
					Scheduled = item.Scheduled,
					Visited = item.Visited,
					ScheduledInvoiced = item.ScheduledInvoiced,
					Unscheduled = item.Unscheduled,
					UnscheduledInvoiced = item.UnscheduledInvoiced,
					Name = empData.FirstOrDefault(x => x.Id == item.SalesmanId).FirstName
				});
            }

			return data.AsQueryable().ToDataSourceResult(0, data.Count(), null, null);
		}
    }
}
