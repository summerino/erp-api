using System.Collections.Generic;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.Inventory
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
