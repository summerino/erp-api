using ERP.Entity;
using ERP.Entity.Catalog;
using ERP.Web.API.Domain.Interfaces.Catalog;

namespace ERP.Web.API.Domain.Services.Catalog;

public class TenantService : ITenantService
{
    private readonly CatalogContext _catalogCtx;

    public TenantService(CatalogContext catalogCtx)
    {
        _catalogCtx = catalogCtx;
    }

    public Tenant FindById(int id)
    {
        return _catalogCtx.Tenants.Find(id);
    }
}