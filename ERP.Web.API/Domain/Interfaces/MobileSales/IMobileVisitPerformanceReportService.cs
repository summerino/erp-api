using ERP.Entity.MobileSales;

namespace ERP.Web.API.Domain.Interfaces.MobileSales;

public interface IMobileVisitPerformanceReportService
{
    IEnumerable<MobileVisitPerformanceReport> GetData(string startDate, string endDate, int? salesId);
}