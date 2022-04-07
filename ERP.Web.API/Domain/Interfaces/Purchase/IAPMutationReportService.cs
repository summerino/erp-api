using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Purchase;

public interface IAPMutationReportService
{
    DataSourceResult GetData(int type, string startDate, string EndDate, string supCode, string status);
}