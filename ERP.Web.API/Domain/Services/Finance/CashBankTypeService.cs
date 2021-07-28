using System.Linq;
using ERP.Entity;
using ERP.Entity.Finance;
using ERP.Web.API.Domain.Interfaces.Finance;

namespace ERP.Web.API.Domain.Services.Finance
{
    public class CashBankTypeService : ICashBankTypeService
    {
        private readonly TenantContext _tenantCtx;

        public CashBankTypeService(TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
        }

        public IQueryable<VwCashBankType> GetLists()
        {
            return _tenantCtx.VwCashBankTypes.Where(x => x.IsActive).OrderBy(x => x.Seq);
        }
    }
}
