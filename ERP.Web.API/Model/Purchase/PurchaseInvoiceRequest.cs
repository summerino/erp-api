using System.Collections.Generic;
using ERP.Entity.Purchase;

namespace ERP.Web.API.Model.Purchase
{
    public class PurchaseInvoiceRequest : PurchaseInvoiceHeader
    {
        public IEnumerable<PurchaseInvoiceDetail> Details { get; set; }
    }
}
