using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IBalanceSheetReportService
    {
        IEnumerable<BalanceSheetResult> GetBalanceSheetLists(IEnumerable<GeneralLedgerResult> data);
    }
}
