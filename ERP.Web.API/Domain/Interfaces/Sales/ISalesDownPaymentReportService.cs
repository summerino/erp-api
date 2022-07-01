using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Sales;

public interface ISalesDownPaymentReportService
{
    DataSourceResult GetData(int type, int? srcTrans, string date, string custCode, string status, IEnumerable<Sort> sorts);
}