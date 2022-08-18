using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileSales;
using ERP.Web.API.Model.MobileSales;

namespace ERP.Web.API.Domain.Interfaces.MobileSales;

public interface IMobileOrderService : IGeneralService<MobileOrderHeader>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
        string search);

    IEnumerable<VwMobileOrderDetail> GetDetailData(string code);

    IEnumerable<MobileDetailFreeGoodData> GetFreeDetailData(string code);

    IEnumerable<MobileOrderDetailDiscount> GetDiscDetailData(string code);

    IEnumerable<dynamic> GetOrderPromos(string code);

    SaveResult Approve(List<MobileOrderHeader> data, int userId, bool allowOverlimit, string reason);

    SaveResult Reject(List<MobileOrderHeader> data, int userId);

    SaveResult Update(MobileOrderRequest data);

    IEnumerable<dynamic> ValidateOverlimit(List<MobileOrderHeader> data);
}