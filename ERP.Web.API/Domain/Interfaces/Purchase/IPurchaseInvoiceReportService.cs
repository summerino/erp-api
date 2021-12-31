using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Purchase
{
    public interface IPurchaseInvoiceReportService
    {
        DataSourceResult GetData(int type, string startDate, string endDate,
            string supCode, string status, int? itemId,
            string code, bool isDetail);
    }
}
