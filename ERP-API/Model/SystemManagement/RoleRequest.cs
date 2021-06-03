using System.Collections.Generic;
using ERP_API.Domain.Entities.SystemManagement;

namespace ERP_API.Model.SystemManagement
{
    public class RoleRequest : Role
    {
        public IEnumerable<RoleMenu> RoleMenus { get; set; }

        public IEnumerable<RoleMenuAction> RoleMenuActions { get; set; }
    }
}
