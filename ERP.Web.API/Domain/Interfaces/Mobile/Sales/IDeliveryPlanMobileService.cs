using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Domain.Models.Mobile.Sales;

namespace ERP.Web.API.Domain.Interfaces.Mobile.Sales
{
    public interface IDeliveryPlanMobileService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date);
        IEnumerable<DeliveryPlanDetailModel> GetDetailData(string code);
        DataSourceResult GetLogData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, string search, string date);
        IEnumerable<VwMobileDeliveryItemDetail> GetLogDetailData(string code);
        SaveResult Insert(DeliveryPlanRequestModel data, int UserId);
    }
}
