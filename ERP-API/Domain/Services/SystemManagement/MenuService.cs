using System;
using System.Collections.Generic;
using System.Linq;
using ERP_API.Domain.Entities;
using ERP_API.Domain.Entities.SystemManagement;
using ERP_API.Domain.Extensions;
using ERP_API.Domain.Interfaces.SystemManagement;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Services.SystemManagement
{
    public class MenuService : GeneralService<Menu>, IMenuService
    {
        public MenuService(TenantContext db)
            : base(db)
        {
        }

        public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search)
        {
            var data = Db.Menus.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                data = data.Where(x => x.Name.Contains(search));

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public object GetHierarchy()
        {
            return new
            {
                Id = 0,
                Name = "Menu",
                ParentId = 0,
                Deep = 0,
                Seq = 0,
                Link = "",
                Icon = "",
                Regex = "",
                Children = DefineChildNodes(Db.Menus.Where(x => x.IsActive).ToList())
            };
        }

        private static object DefineChildNodes(List<Menu> data, int? parentId = null)
        {
            var nodes = data
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Seq)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.ParentId,
                    x.Deep,
                    x.Seq,
                    x.Link,
                    x.Icon,
                    x.Regex,
                    Children = DefineChildNodes(data, x.Id)
                });

            return nodes;
        }

        public IEnumerable<ERP_API.Domain.Entities.SystemManagement.Action> GetActions()
        {
            var data = Db.Actions.AsQueryable();

            return data.OrderBy(x => x.Id);
        }

        public IEnumerable<MenuAction> GetLists(int id)
        {
            var data = Db.MenuActions.Where(x => x.MenuId == id);

            return data.OrderBy(x => x.Id);
        }
    }
}
