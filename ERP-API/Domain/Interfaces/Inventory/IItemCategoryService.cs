using System.Collections.Generic;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;
using Swift.Framework.Model;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IItemCategoryService : IGeneralService<ItemCategory>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        object GetHierarchy();
    }
}
