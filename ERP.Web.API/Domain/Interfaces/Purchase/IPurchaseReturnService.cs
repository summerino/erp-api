using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Purchase;
using ERP.Web.API.Domain.Models;
using ERP.Web.API.Model.Purchase;

namespace ERP.Web.API.Domain.Interfaces.Purchase
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
