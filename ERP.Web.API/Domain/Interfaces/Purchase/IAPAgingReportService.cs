using ERP.Common.Models;

namespace ERP.Web.API.Domain.Interfaces.Purchase;

public interface IAPAgingReportService
{
    DataSourceResult GetData(int type, string date, string supCode, string duration);
}