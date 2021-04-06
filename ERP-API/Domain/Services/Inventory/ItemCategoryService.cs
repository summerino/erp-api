using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Interfaces.Inventory;

namespace ERP_API.Domain.Services.Inventory
{
    public class ItemCategoryService: IItemCategoryService
    {
        private readonly TenantContext _tenantCtx;

        public ItemCategoryService(TenantContext tenantCtx)
        {
            _tenantCtx = tenantCtx;
        }

        public IEnumerable<ItemCategory> GetData()
        {
            return _tenantCtx.ItemCategories.Where(x => x.IsActive);
        }

        public object GetHierarchy()
        {
            return new
            {
                Id = 0,
                Name = "All Category",
                Children = DefineChildNodes(_tenantCtx.ItemCategories.Where(x => x.IsActive).ToList())
            };
        }

        private static object DefineChildNodes(List<ItemCategory> data, int? parentId = null)
        {
            var nodes = data
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Seq)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    Children = DefineChildNodes(data, x.Id)
                });

            return nodes;
        }
    }
}
