using System.Collections.Generic;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IItemService : IGeneralService<Item>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        SaveResult Delete(int id, int userId);
    }
}
