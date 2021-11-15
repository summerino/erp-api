using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IJournalReportService
    {
        IEnumerable<ReportJournalResult> GetLists(string rptBy, string dateFrom, string dateTo,
            string vouFrom, string rptDet, string src, string coaCode, string sort);
    }
}
