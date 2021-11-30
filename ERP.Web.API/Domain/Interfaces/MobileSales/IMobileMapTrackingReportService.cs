using ERP.Entity.Sales;

namespace ERP.Web.API.Domain.Interfaces.MobileSales
{
    public interface IMobileMapTrackingReportService
    {
        IEnumerable<SalesmanMapTrackingHistory> GetData(int salesId, int type, string startDate, string endDate);
    }
}
