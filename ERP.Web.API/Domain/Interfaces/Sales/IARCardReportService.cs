using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface IARCardReportService
    {
        DataSourceResult GetData(string startDate, string endDate, string custCode, int? salesId);
    }
}
