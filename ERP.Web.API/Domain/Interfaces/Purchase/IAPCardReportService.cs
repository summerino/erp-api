using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Purchase
{
    public interface IAPCardReportService
    {
        DataSourceResult GetData(string startDate, string endDate, string supCode);
    }
}
