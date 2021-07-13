using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Models;

namespace ERP.Web.API.Domain.Interfaces.Inventory
{
    public interface IItemCategoryService : IGeneralService<ItemCategory>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwItemCategory> GetLists();

        object GetHierarchy();

        SaveResult Delete(int id, int userId);
    }
}
