using System.Collections.Generic;
using ERP.Entity.Purchase;

namespace ERP_API.Model.Purchase
{
    public class PurchaseInvoiceRequest : PurchaseInvoiceHeader
    {
        public IEnumerable<PurchaseInvoiceDetail> Details { get; set; }
    }
}
