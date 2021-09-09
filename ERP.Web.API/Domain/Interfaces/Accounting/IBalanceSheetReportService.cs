using ERP.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IBalanceSheetReportService
    {
        IEnumerable<BalanceSheetResult> GetBalanceSheetLists(IEnumerable<GeneralLedgerResult> data);
    }
}
