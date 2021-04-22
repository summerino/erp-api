using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Model.Purchase;

namespace ERP_API.Domain.Interfaces.Purchase
{
    public interface IPurchaseOrderService : IGeneralService<PurchaseOrderHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts,
            string search);

        IEnumerable<VwPurchaseOrderDetail> GetDetailData(string code, bool? fullReceived = null);

        List<dynamic> GetRelatedTransactions(string code);

        SaveResult Insert(PurchaseOrderRequest data);

        SaveResult Update(PurchaseOrderRequest data);

        SaveResult Delete(string code, int userId);
    }
}
