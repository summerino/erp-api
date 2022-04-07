using ERP.Entity;
using ERP.Entity.Finance;
using ERP.Web.API.Domain.Interfaces.Finance;

namespace ERP.Web.API.Domain.Services.Finance;

public class CashBankTypeService : ICashBankTypeService
{
    private readonly TenantContext _tenantCtx;

    public CashBankTypeService(TenantContext tenantCtx)
    {
        _tenantCtx = tenantCtx;
    }

    public IQueryable<VwCashBankType> GetLists(List<int> actionId)
    {
        var data = _tenantCtx.VwCashBankTypes.Where(x => x.IsActive);

        if (actionId?.Any() ?? false)
        {
            data = data.Where(x => actionId.Contains(x.ActionId));
        }

        return data.OrderBy(x => x.Seq);
    }
}