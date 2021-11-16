using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.MobileSales
{
    public interface IMobileVisitPerformanceReportService
    {
        DataSourceResult GetData(int? salesId, string startDate, string endDate);
    }
}
