using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Purchase
{
    public interface IPurchaseOrderReportService
    {
        DataSourceResult GetData(int type, string startDate, string endDate,
            string supCode, string status, int? itemId,
            string code, bool isDetail, int? unitId,
            int? categoryId);
    }
}
