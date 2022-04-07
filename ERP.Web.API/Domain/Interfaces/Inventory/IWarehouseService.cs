using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Inventory;

namespace ERP.Web.API.Domain.Interfaces.Inventory;

public interface IWarehouseService : IGeneralService<Warehouse>
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
        List<int> category, string search);

    DataSourceResult GetLists(IEnumerable<Filter> filters, IEnumerable<Sort> sorts);

    SaveResult Delete(string code, int userId);
}