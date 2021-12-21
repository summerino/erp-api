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

        public IEnumerable<SalesmanMapTrackingHistory> GetData(string date, int salesId, int type)
        {
            var data = 
                _db.SalesmanMapTrackingHistories
                    .Where(x => x.TrackedDate.Date == Convert.ToDateTime(date) && x.SalesmanId == salesId);

            return type == 1
                ? data.OrderBy(y => y.TrackedDate)
                : data.OrderByDescending(y => y.TrackedDate).Take(1);
        }

        public IEnumerable<object> GetCustomerData(string date, int salesId)
        {
            var visitOrderCustomers =
                _db.VisitOrderCustomers
                    .Where(voc =>
                        _db.VisitOrders
                            .Where(vo => vo.Date == Convert.ToDateTime(date) && vo.SalesmanId == salesId && vo.Mark == "A")
                            .Select(vo => vo.Code).Contains(voc.Code));

            var customerAddresses =
                _db.CustomerAddress
                    .Where(ca => visitOrderCustomers.Select(voc => voc.CustCode).Contains(ca.Code) && ca.Lat.HasValue && ca.Lng.HasValue);

            var data = from ca in customerAddresses
                join c in _db.Customers on ca.Code equals c.Code
                select new { c.Code, c.Name, ca.Lat, ca.Lng };

            return data;
        }
    }
}
