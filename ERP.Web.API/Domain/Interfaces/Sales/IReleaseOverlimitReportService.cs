using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales
{
    public interface IReleaseOverlimitReportService
    {
        DataSourceResult GetData(string startDate, string endDate, string releasedBy, string custCode, IEnumerable<Sort> sorts);
    }
}
