using ERP.Entity.Finance;

namespace ERP.Web.API.Domain.Interfaces.Finance;

public interface ICashFlowReportService
{
    IEnumerable<CashFlowReportAll> GetData(int type, string dateFrom, string dateTo);

    IEnumerable<CashFlowReportByCOA> GetDataCOA(int type, string coa, string dateFrom, string dateTo);
}