using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Inventory;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Inventory;

namespace ERP.Web.API.Domain.Interfaces.Inventory
{
    public interface IItemGroupService : IGeneralService<ItemGroup>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        IEnumerable<ItemGroupSubGroup> GetDetailData(int id);

        IEnumerable<ItemGroupSubGroup> GetDetailById(int id);

        IEnumerable<ItemGroup> GetLists();

        SaveResult Insert(ItemGroupRequest data);

        SaveResult Update(ItemGroupRequest data);

        SaveResult Delete(int id, int userId);
    }
}
