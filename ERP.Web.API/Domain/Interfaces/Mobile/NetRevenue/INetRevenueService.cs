using ERP.Common.Models;
using ERP.Web.API.Domain.Models.Mobile.NetRevenue;
using ERP.Web.API.Domain.Models.Mobile.Operational;

namespace ERP.Web.API.Domain.Interfaces.Mobile.NetRevenue;

public interface INetRevenueService
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, int userId, string date);
    IEnumerable<NetRevenueDetailModel> GetRevenueDetail(string date, int userId);
    IEnumerable<CostDetailModel> GetCostDetail(string date, int userId);
    DataSourceResult GetRevenueDetailByCoa(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string date, string coaCode, int userId);
}