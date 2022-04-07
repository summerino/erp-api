using ERP.Common.Models;
using ERP.Entity.Inventory;

namespace ERP.Web.API.Domain.Interfaces.Inventory;

public interface IWarehouseQuantityService : IGeneralService<WarehouseQuantity>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        List<int> category, string search);
}