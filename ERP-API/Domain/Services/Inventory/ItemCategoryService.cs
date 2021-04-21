using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.Inventory;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.Inventory
{
    public class ItemCategoryService: GeneralService<ItemCategory>, IItemCategoryService
    {
        public ItemCategoryService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.ItemCategories.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = 
                    data.Where(x =>
                        x.Initial.Contains(search) || x.Name.Contains(search) || x.GroupId.Contains(search));
            }

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public object GetHierarchy()
        {
            return new
            {
                Id = 0,
                Name = "All Category",
                Children = DefineChildNodes(Db.ItemCategories.Where(x => x.IsActive).ToList())
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
