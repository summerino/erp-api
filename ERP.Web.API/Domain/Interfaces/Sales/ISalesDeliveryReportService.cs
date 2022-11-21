using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales;

public interface ISalesDeliveryReportService
{
    DataSourceResult GetData(int type, int? srcTrans, string startDate, string endDate,
        int? salesId, string custCode, string status, int? itemId,
        string code, bool isDetail, int? unitId,
        int? categoryId);
}