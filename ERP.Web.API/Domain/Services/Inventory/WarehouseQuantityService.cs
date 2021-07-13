using System.Collections.Generic;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Domain.Models;

namespace ERP.Web.API.Domain.Services.Inventory
{
    public class WarehouseQuantityService : GeneralService<WarehouseQuantity>, IWarehouseQuantityService
    {
        public WarehouseQuantityService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search)
        {
            return Db.VwWarehouseQuantities.ToDataSourceResult(skip, take, filter, sort);
        }
    }
}
