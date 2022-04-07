using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales;

public interface ISalesReturnReportService
{
    DataSourceResult GetData(int type, string startDate, string endDate,
        string custCode, string status, int? itemId,
        string code, bool isDetail, int? unitId,
        int? categoryId);
}