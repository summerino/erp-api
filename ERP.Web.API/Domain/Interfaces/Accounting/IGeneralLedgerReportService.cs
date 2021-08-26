using ERP.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IGeneralLedgerReportService
    {
        IEnumerable<GeneralLedgerResult> GetGeneralLedgerLists(string dateFrom, string dateTo,
            string coaFrom, string coaTo, string currCode, string sort, int? caller);
    }
}
