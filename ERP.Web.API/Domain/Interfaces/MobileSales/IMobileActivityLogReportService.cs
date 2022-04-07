using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.MobileSales;

public interface IMobileActivityLogReportService
{
    DataSourceResult GetData(string username, string typeCode, string note, string startDate, string endDate);
}