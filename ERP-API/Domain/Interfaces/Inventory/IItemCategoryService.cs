using System.Collections.Generic;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IItemCategoryService : IGeneralService<ItemCategory>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<ItemCategory> GetLists();

        object GetHierarchy();

        SaveResult Delete(int id, int userId);
    }
}
