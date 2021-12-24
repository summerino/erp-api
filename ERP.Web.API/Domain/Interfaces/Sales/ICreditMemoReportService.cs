using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface ICreditMemoReportService
    {
        DataSourceResult GetData(int type, string date, string custCode, string status, IEnumerable<Sort> sorts);
    }
}
