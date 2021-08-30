using ERP.Entity.Catalog;

namespace ERP.Web.API.Domain.Interfaces.Catalog
{
    public interface ITenantService
    {
        Tenant FindById(int id);
    }
}
