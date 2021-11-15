using ERP.Entity.Accounting;

namespace ERP.Web.API.Domain.Interfaces.Accounting
{
    public interface IIncomeStatementReportService
    {
        IEnumerable<IncomeStatementResult> GetIncomeStatementLists(string periodType, string rptBy,
            string rptDet, string dateTo);

        IEnumerable<BsIsDetailResult> GetBsIsDetailLists(string typeFormat, string code, string plusMinus,
            string dateFrom, string dateTo, string jourSrc);
    }
}
