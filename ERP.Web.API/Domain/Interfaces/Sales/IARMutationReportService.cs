using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales;

public interface IARMutationReportService
{
    DataSourceResult GetData(int type, string startDate, string endDate, string custCode, int slsId, string status);
}