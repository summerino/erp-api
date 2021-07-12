using System.Collections.Generic;
using ERP.Entity.Inventory;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IWarehouseQuantityService : IGeneralService<WarehouseQuantity>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);
    }
}
