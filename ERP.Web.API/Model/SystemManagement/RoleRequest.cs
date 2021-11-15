using ERP.Entity.SystemManagement;

namespace ERP.Web.API.Model.SystemManagement
{
    public class RoleRequest : Role
    {
        public IEnumerable<RoleMenu> RoleMenus { get; set; }

        public IEnumerable<RoleMenuAction> RoleMenuActions { get; set; }
    }
}
