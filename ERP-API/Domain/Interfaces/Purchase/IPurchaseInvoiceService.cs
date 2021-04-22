using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;
using ERP_API.Domain.Models;
using ERP_API.Model.Purchase;

namespace ERP_API.Domain.Interfaces.Purchase
{
    public interface IPurchaseInvoiceService : IGeneralService<PurchaseInvoiceHeader>
    {
        DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort,
            string search);

        IEnumerable<PurchaseInvoiceDetail> GetDetailData(string code);

        SaveResult Insert(PurchaseInvoiceRequest data);

        SaveResult Update(PurchaseInvoiceRequest data);

        SaveResult Delete(string code, int userId);
    }
}
