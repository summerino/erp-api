using ERP.Entity.MobileSales;

namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder
{
    public class VisitSubmitModel : MobileVisitLog
    {
        public IEnumerable<int> VisitReasons { get; set; }
        public IEnumerable<PaymentInvoiceRequestModel> Invoices { get; set; }
        public IEnumerable<OrderHeaderSubmitModel> OrderHeaders { get; set; }
    }
}
