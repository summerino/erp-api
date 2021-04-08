using System.Collections.Generic;
using ERP_API.Domain.Models;
using ERP_API.Model;
using Swift.Framework.Model;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IItemService
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category);
    }
}
