using System.Collections.Generic;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model.Inventory;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IItemGroupService : IGeneralService<ItemGroup>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        IEnumerable<ItemGroupSubGroup> GetDetailData(int id);

        IEnumerable<ItemGroup> GetItemGroup();

        SaveResult Insert(ItemGroupRequest data);

        SaveResult Update(ItemGroupRequest data);

        SaveResult Delete(int id, int userId);
    }
}
