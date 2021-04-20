using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;

namespace ERP_API.Model.Purchase
{
    public class PurchaseInvoiceRequest : PurchaseInvoiceHeader
    {
        public IEnumerable<PurchaseInvoiceDetail> Details { get; set; }
    }
}
