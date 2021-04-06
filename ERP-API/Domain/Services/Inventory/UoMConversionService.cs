using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Interfaces.Inventory;

namespace ERP_API.Domain.Services.Inventory
{
    public class UoMConversionService : IUoMConversionService
    {
        private readonly TenantContext _tenantCtx;

        public UoMConversionService(TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
        }

        public IEnumerable<UoMConversion> GetData(int? uomId)
        {
            var data = _tenantCtx.UoMConversions.AsQueryable();

            if (uomId.HasValue)
                data = data.Where(x => x.UomId == uomId);

            return data.OrderBy(x => x.Seq);
        }
    }
}
