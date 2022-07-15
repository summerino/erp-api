using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.Sales
{
    public class ReleaseOverlimitReportService : IReleaseOverlimitReportService
    {
        private readonly TenantContext _db;
        public ReleaseOverlimitReportService(TenantContext db)
        {
            _db = db;
        }

        public DataSourceResult GetData(string startDate, string endDate, string releasedBy, string custCode, IEnumerable<Sort> sorts)
        {
            var data = _db.ReleaseOverlimitReports.FromSqlRaw(@"SELECT Code, OverlimitApprovedDate AS ReleasedDate,
                        OverlimitApprovedInitial AS ReleasedBy, OverlimitApprovedReason AS ReleasedReason, CustCode, CustName, Total
                        FROM Sales.VwSalesOrderHeader
                        WHERE OverlimitApprovedBy IS NOT NULL AND OverlimitApprovedDate IS NOT NULL" +
                        (string.IsNullOrEmpty(releasedBy) ? "" : $" AND OverlimitApprovedBy = '{releasedBy.Replace("'", "''")}'") +
                        (string.IsNullOrEmpty(custCode) ? "" : $" AND CustCode = '{custCode.Replace("'", "''")}'")).ToList();

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                data = data.Where(x => x.ReleasedDate >= Convert.ToDateTime(startDate) && x.ReleasedDate <= Convert.ToDateTime(endDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(startDate))
            {
                data = data.Where(x => x.ReleasedDate >= Convert.ToDateTime(startDate)).ToList();
            }
            else if (!string.IsNullOrEmpty(endDate))
            {
                data = data.Where(x => x.ReleasedDate <= Convert.ToDateTime(endDate)).ToList();
            }

            return data.AsQueryable().ToDataSourceResult(0, data.Count, null, sorts);
        }
    }
}
