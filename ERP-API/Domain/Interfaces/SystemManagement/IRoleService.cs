using System.Collections.Generic;
using ERP.Entity.SystemManagement;
using ERP_API.Domain.Models;
using ERP_API.Model.SystemManagement;

namespace ERP_API.Domain.Interfaces.SystemManagement
{
    public interface IRoleService : IGeneralService<Role>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string search);

        object GetRoleMenu(int id);

        IEnumerable<RoleMenuAction> GetRoleMenuAction(int id);

        SaveResult Insert(RoleRequest data);

        SaveResult Update(RoleRequest data, int id);

        SaveResult Delete(int id, int userId);
    }
}
