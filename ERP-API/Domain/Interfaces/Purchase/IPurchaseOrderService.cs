using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Model.Purchase;

namespace ERP_API.Domain.Interfaces.Purchase
{
    public interface IPurchaseOrderService : IGeneralService<PurchaseOrderHeader>
    {
        IEnumerable<VwPurchaseOrderDetail> GetDetailData(string code);

        SaveResult Insert(PurchaseOrderRequest data);

        SaveResult Update(PurchaseOrderRequest data);

        SaveResult Delete(string code, int userId);
    }
}
