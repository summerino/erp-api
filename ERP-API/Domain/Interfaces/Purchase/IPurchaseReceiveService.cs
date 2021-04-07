using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Model.Purchase;

namespace ERP_API.Domain.Interfaces.Purchase
{
    public interface IPurchaseReceiveService : IGeneralService<PurchaseReceiveHeader>
    {
        IEnumerable<VwPurchaseReceiveDetail> GetDetailData(string code);

        SaveResult Insert(PurchaseReceiveRequest data);

        SaveResult Update(PurchaseReceiveRequest data);

        SaveResult Delete(string code, int userId);
    }
}
