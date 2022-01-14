using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Finance
{
    public interface IOutstandingChequeReportService
    {
        DataSourceResult GetData(string date, string coaCode);
    }
}
