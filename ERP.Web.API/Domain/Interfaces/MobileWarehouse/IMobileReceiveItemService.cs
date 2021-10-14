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
    public interface IMobileReceiveItemService : IGeneralService<MobileReceiveItemHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwMobileReceiveItemDetail> GetDetailData(string code);

        SaveResult Approve(List<MobileReceiveItemHeader> data, int userId);

        SaveResult Reject(List<MobileReceiveItemHeader> data, int userId);

        SaveResult Update(MobileReceiveItemRequest data);
    }
}
