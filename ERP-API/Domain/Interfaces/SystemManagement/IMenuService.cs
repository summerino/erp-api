using System.Collections.Generic;
using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.SystemManagement
{
    public interface IMenuService : IGeneralService<Menu>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<Action> GetActions();

        IEnumerable<MenuAction> GetLists(int id);

        object GetHierarchy();
    }
}
