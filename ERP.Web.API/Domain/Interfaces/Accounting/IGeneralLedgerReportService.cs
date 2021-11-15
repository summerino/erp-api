using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IGeneralLedgerReportService
    {
        IEnumerable<GeneralLedgerResult> GetGeneralLedgerLists(string dateFrom, string dateTo,
            string coaFrom, string coaTo, string currCode, string sort, int? caller);
    }
}
