using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Model.Purchase;

namespace ERP_API.Domain.Interfaces.Purchase
{
    public interface IPurchaseReturnService : IGeneralService<PurchaseReturnHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        IEnumerable<VwPurchaseReturnDetail> GetDetailData(string code, bool? fullReceived = null);

        IEnumerable<VwPurchaseReturnDetailExchDiffItem> GetDetailExchangeData(string code);

        List<dynamic> GetRelatedTransactions(string code);

        SaveResult Insert(PurchaseReturnRequest data);

        SaveResult Update(PurchaseReturnRequest data);

        SaveResult Delete(string code, int userId);
    }
}
