using ERP_API.Domain.Interfaces.Finance;
using System.Linq;
using ERP.Entity;
using ERP.Entity.Finance;

namespace ERP_API.Domain.Services.Finance
{
    public class CashBankTypeService : ICashBankTypeService
    {
        private readonly TenantContext _tenantCtx;

        public CashBankTypeService(TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
        }
        public IQueryable<CashBankType> GetList()
        {
            return _tenantCtx.CashBankTypes.Where(x=>x.IsActive).OrderBy(x => x.Seq);
        }
    }
}
