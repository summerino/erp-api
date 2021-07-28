using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Purchase;
using ERP.Web.API.Model.Purchase;

namespace ERP.Web.API.Domain.Interfaces.Purchase
{
    public interface IPurchaseReceiveService : IGeneralService<PurchaseReceiveHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        IEnumerable<VwPurchaseReceiveDetail> GetDetailData(string code);

        List<dynamic> GetRelatedTransactions(string code);

        IEnumerable<PurchaseReceiveHeader> GetUnInvoiceData(string poCode, string invCode);

        SaveResult Insert(PurchaseReceiveRequest data);

        SaveResult Update(PurchaseReceiveRequest data);

        SaveResult Delete(string code, int userId);
    }
}
