using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface IARAgingReportService
    {
        DataSourceResult GetData(int type, string date, string custCode, int? slsId, string duration);
    }
}
