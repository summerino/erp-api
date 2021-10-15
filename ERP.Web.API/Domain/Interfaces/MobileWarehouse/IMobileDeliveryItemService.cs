using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.MobileWarehouse;
using ERP.Web.API.Model.MobileWarehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Interfaces.MobileWarehouse
{
    public interface IMobileDeliveryItemService : IGeneralService<MobileDeliveryItemHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwMobileDeliveryItemDetail> GetDetailData(string code);

        SaveResult Approve(List<MobileDeliveryItemHeader> data, int userId);

        SaveResult Reject(List<MobileDeliveryItemHeader> data, int userId);

        SaveResult Update(MobileDeliveryItemRequest data);
    }
}
