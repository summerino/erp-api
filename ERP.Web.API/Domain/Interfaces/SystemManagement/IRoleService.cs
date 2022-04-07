using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.SystemManagement;
using ERP.Web.API.Model.SystemManagement;

namespace ERP.Web.API.Domain.Interfaces.SystemManagement;

public interface IRoleService : IGeneralService<Role>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        List<int> category, string search);

    object GetRoleMenu(int id, int? menuId = null);

    IEnumerable<RoleMenuAction> GetRoleMenuAction(int id);

    SaveResult Insert(RoleRequest data);

    SaveResult Update(RoleRequest data, int id);

    SaveResult Delete(int id, int userId);
}