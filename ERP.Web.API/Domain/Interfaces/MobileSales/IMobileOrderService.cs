using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileSales;
using ERP.Web.API.Model.MobileSales;
using ERP.Web.API.Model.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.MobileSales
{
    public interface IMobileOrderService : IGeneralService<MobileOrderHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwMobileOrderDetail> GetDetailData(string code);

        IEnumerable<MobileDetailFreeGoodData> GetFreeDetailData(string code);

        IEnumerable<MobileOrderDetailDiscount> GetDiscDetailData(string code);

        SaveResult Approve(List<MobileOrderHeader> data, int userId);

        SaveResult Reject(List<MobileOrderHeader> data, int userId);

        SaveResult Update(MobileOrderRequest data);
    }
}
