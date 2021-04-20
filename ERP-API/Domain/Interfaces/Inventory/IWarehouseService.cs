using System.Collections.Generic;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;
using Swift.Framework.Model;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IWarehouseService : IGeneralService<Warehouse>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        SaveResult Delete(string code, int userId);
    }
}
