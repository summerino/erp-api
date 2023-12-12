using ERP.Common.Models;
using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting;

public interface IGeneralLedgerReportService
{
    IEnumerable<GeneralLedgerResult> GetGeneralLedgerLists(string dateFrom, string dateTo,
        string coaFrom, string coaTo, string currCode, string sort, int? caller);

    Task<Stream> GetDataExcel(IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search,
        IEnumerable<JournalReportWrapper> wrappedData, bool isMain, string title);
}