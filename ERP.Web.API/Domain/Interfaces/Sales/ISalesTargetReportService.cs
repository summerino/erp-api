using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales;

public interface ISalesTargetReportService
{
    DataSourceResult GetData(string startDate, string endDate,
        int? salesId, int? groupId, int? groupSubGroupId,
        string groupSubGroup, bool isDetail);
}