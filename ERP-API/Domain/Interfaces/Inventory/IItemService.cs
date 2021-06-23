using System.Collections.Generic;
using ERP_API.Domain.Entities.Inventory;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Entities.Sales;
using ERP_API.Domain.Models;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface IItemService : IGeneralService<Item>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            List<int> category, string warehouseCode, string search);

        SaveResult Delete(int id, int userId);

        IEnumerable<dynamic> GetRelatedOrderTrans(string whid, int itemid);

        IEnumerable<dynamic> GetRelatedIndentTrans(string whid, int itemid);

        IEnumerable<dynamic> GetRelatedTransferTrans(string whid, int itemid);
    }
}
