using System.Collections.Generic;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model;
using Swift.Framework.Model;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IItemService : IGeneralService<Item>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, string search);

        SaveResult Delete(string Initial, int userId);
    }
}
