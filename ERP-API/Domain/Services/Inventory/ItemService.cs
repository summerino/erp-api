using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model;
using Swift.Framework.Model;

namespace ERP_API.Domain.Services.Inventory
{
    public class ItemService : IItemService
    {
        private readonly TenantContext _tenantCtx;

        public ItemService(TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category)
        {
            var data = _tenantCtx.VwItems.Where(x => x.IsActive);

            if (category?.Any() ?? false)
            {
                data = data.Where(x => category.Contains(x.CategoryId));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }
    }
}
