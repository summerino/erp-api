using ERP.Entity;
using ERP.Entity.Sales;
using ERP.Web.API.Domain.Interfaces.MobileSales;

namespace ERP.Web.API.Domain.Services.MobileSales
{
    public class MobileMapTrackingReportService : IMobileMapTrackingReportService
    {
        private readonly TenantContext _db;
        public MobileMapTrackingReportService(TenantContext db)
        {
            _db = db;
        }
        public IEnumerable<SalesmanMapTrackingHistory> GetData(int salesId, int type, string startDate, string endDate)
        {
            var data = _db.SalesmanMapTrackingHistories.ToList();

            data = data.Where(x => x.SalesmanId == salesId).ToList();

            if (type == 1)
            {
                if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                {
                    data = data.Where(x => x.TrackedDate >= Convert.ToDateTime(startDate) && x.TrackedDate <= Convert.ToDateTime(endDate)).ToList();
                }
                else if (!string.IsNullOrEmpty(startDate))
                {
                    data = data.Where(x => x.TrackedDate >= Convert.ToDateTime(startDate)).ToList();
                }
                else if (!string.IsNullOrEmpty(endDate))
                {
                    data = data.Where(x => x.TrackedDate <= Convert.ToDateTime(endDate)).ToList();
                }

                data = data.OrderByDescending(y => y.TrackedDate).ToList();

                return data;
            } 
            else
            {
                List<SalesmanMapTrackingHistory> result = new();
                var lastData = data.OrderByDescending(y => y.TrackedDate).FirstOrDefault();
                if (lastData != null)
                    result.Add(lastData);
                
                return result;  
            }
        }
    }
}
