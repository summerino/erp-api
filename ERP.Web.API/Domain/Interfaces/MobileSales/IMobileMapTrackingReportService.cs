using ERP.Entity.Sales;

namespace ERP.Web.API.Domain.Interfaces.MobileSales
{
    public interface IMobileMapTrackingReportService
    {
        IEnumerable<SalesmanMapTrackingHistory> GetData(string date, int salesId, int type);
        
        IEnumerable<object> GetCustomerData(string date, int salesId);
    }
}
