using ERP_API.Domain.Entities;
using ERP_API.Domain.Interfaces.Finance;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.Finance
{
    public class CashBankTypeService : ICashBankTypeService
    {
        private readonly TenantContext _tenantCtx;

        public CashBankTypeService(TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
        }
        public DataSourceResult GetList()
        {
            throw new System.NotImplementedException();
        }
    }
}
