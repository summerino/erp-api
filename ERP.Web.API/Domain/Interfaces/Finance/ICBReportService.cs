using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Finance;

public interface ICBReportService
{
    DataSourceResult GetData(int? type, string startDate, string endDate, string coaCode, IEnumerable<Sort> sorts);
}