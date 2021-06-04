using System.Collections.Generic;
using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Models;
using ERP_API.Domain.Models.SystemManagement;

namespace ERP_API.Domain.Interfaces.SystemManagement
{
    public interface IMenuService : IGeneralService<Menu>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<Action> GetActions();

        IEnumerable<MenuAction> GetLists(int id);

        IEnumerable<MenuNavigation> GetNavigation(int roleId);

        object GetHierarchy();
    }
}
