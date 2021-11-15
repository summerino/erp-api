using ERP.Common.Models;
using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Models.SystemManagement;

namespace ERP.Web.API.Domain.Interfaces.SystemManagement
{
    public interface IMenuService : IGeneralService<Menu>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<Entity.SystemManagement.Action> GetActions();

        IEnumerable<MenuAction> GetLists(int id);

        IEnumerable<MenuNavigation> GetNavigation(int roleId);

        object GetHierarchy();
    }
}
