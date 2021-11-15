using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.SystemManagement;

namespace ERP.Web.API.Domain.Services.SystemManagement
{
    public class ActionService : IActionService
    {
        private readonly TenantContext _tenantCtx;

        public ActionService(TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
        }

        public IEnumerable<Entity.SystemManagement.Action> GetData()
        {
            return _tenantCtx.Actions;
        }
    }
}
