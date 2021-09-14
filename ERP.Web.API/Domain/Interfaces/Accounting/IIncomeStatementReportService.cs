using ERP.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IIncomeStatementReportService
    {
        IEnumerable<IncomeStatementResult> GetIncomeStatementLists(string periodType, string rptBy,
            string rptDet, string dateTo);

        IEnumerable<BsIsDetailResult> GetBsIsDetailLists(string typeFormat, string code, string PlusMinus,
            string dateFrom, string dateTo, string jourSrc);
    }
}
