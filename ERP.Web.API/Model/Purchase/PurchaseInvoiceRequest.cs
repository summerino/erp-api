using ERP.Entity.Purchase;

namespace ERP.Web.API.Model.Purchase
{
    public class PurchaseInvoiceRequest : PurchaseInvoiceHeader
    {
        public IEnumerable<PurchaseInvoiceDetail> Details { get; set; }
        public IEnumerable<PurchaseInvoiceDebitMemo> Memos { get; set; }

        public DateTime? OriginalDate { get; set; }

        public DateTime? OriginalDueDate { get; set; }
    }
}
