using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Purchase;

public interface IDebitMemoReportService
{
    DataSourceResult GetData(int type, string date, string supCode, string status, IEnumerable<Sort> sorts);
}