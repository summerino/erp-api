using System.Collections.Generic;
using ERP.Entity.Inventory;
using ERP_API.Domain.Models;
using ERP_API.Model.Inventory;

namespace ERP_API.Domain.Interfaces.Inventory
{
    public interface ITransferStockService : IGeneralService<TransferStockHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwTransferStockDetail> GetDetailData(string code);

        List<dynamic> GetRelatedTransactions(string code);

        SaveResult Insert(TransferStockRequest data);

        SaveResult Update(TransferStockRequest data);

        SaveResult Delete(string code, int userId);
    }
}
