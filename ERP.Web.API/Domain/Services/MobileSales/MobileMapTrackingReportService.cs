using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.MobileSales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Web.API.Domain.Services.MobileSales
{
    public class MobileMapTrackingReportService : IMobileMapTrackingReportService
    {
        private readonly TenantContext _db;

        public MobileMapTrackingReportService(TenantContext db)
        {
            _db = db;
        }

        public IEnumerable<SalesmanMapTrackingHistory> GetData(string date, int salesId, int type)
        {
            var data = 
                _db.SalesmanMapTrackingHistories
                    .Where(x => x.TrackedDate.Date == Convert.ToDateTime(date) && x.SalesmanId == salesId);

            return type == 1
                ? data.OrderBy(y => y.TrackedDate)
                : data.OrderByDescending(y => y.TrackedDate).Take(1);
        }
    }
}
