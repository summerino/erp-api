using ERP.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface ITrialBalanceReportService
    {
        IEnumerable<TrialBalanceResult> GetTrialBalanceLists(string rptBy, string dateFrom, string dateTo, string currCode);
    }
}
