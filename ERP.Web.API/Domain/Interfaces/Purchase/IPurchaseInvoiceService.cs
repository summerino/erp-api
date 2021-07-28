using System.Collections.Generic;
using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.Purchase;
using ERP.Web.API.Model.Purchase;

namespace ERP.Web.API.Domain.Interfaces.Purchase
{
    public interface IPurchaseInvoiceService : IGeneralService<PurchaseInvoiceHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        IEnumerable<PurchaseInvoiceDetail> GetDetailData(string code);

        List<dynamic> GetRelatedTransactions(string code);

        SaveResult Insert(PurchaseInvoiceRequest data);

        SaveResult Update(PurchaseInvoiceRequest data);

        SaveResult Delete(string code, int userId);
    }
}
