using System.Collections.Generic;
using System.Linq;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.SystemManagement;
using ERP.Web.API.Domain.Interfaces.SystemManagement;
using ERP.Web.API.Domain.Models.SystemManagement;

namespace ERP.Web.API.Domain.Services.SystemManagement
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

        public IEnumerable<MenuNavigation> GetNavigation(int roleId)
        {
            // Get menu based on role
            var roleMenus = Db.RoleMenus.Where(x => x.IsActive && x.RoleId == roleId).Select(x => x.MenuId);
            var menus = Db.Menus.Where(x => x.IsActive && roleMenus.Contains(x.Id)).ToList();

            // Define navigation nodes
            var navigation = menus
                .Where(x => x.Deep == 0)
                .OrderBy(x => x.Seq)
                .Select(x => new MenuNavigation
                {
                    Icon = x.Icon,
                    Text = x.Name,
                    Link = x.Link,
                    Regex = x.Regex,
                    Items = DefineChildNavigation(menus, x.Id)
                }).ToList();

            // Adding Dashboard for default navigation
            navigation.Insert(0, new MenuNavigation
            {
                Items = new List<MenuNavigation>
                {
                    new()
                    {
                        Icon = "mdi-view-dashboard-outline",
                        Text = "Dashboard",
                        Link = "dashboard"
                    }
                }
            });

            return navigation;
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

        public IEnumerable<Action> GetActions()
        {
            var data = Db.Actions.AsQueryable();

            return data.OrderBy(x => x.Id);
        }

        public IEnumerable<MenuAction> GetLists(int id)
        {
            var data = Db.MenuActions.Where(x => x.MenuId == id);

            return data.OrderBy(x => x.Id);
        }

        private static IEnumerable<MenuNavigation> DefineChildNavigation(List<Menu> data, int? parentId = null)
        {
            return data
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Seq)
                .Select(x => new MenuNavigation
                {
                    Icon = x.Icon,
                    Text = x.Name,
                    Link = x.Link,
                    Regex = x.Regex,
                    Items = DefineChildNavigation(data, x.Id)
                });
        }
    }
}
