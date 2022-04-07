using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting;

public interface ITrialBalanceReportService
{
    IEnumerable<TrialBalanceResult> GetTrialBalanceLists(string rptBy, string dateFrom, string dateTo, string currCode);
}