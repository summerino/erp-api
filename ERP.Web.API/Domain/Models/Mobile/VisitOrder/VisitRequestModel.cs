using System.Collections.Generic;
using ERP.Entity.MobileSales;

namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder
{
    public class VisitRequestModel : MobileVisitLog
    {
        public IEnumerable<int> VisitReasons { get; set; }
        public IEnumerable<PaymentInvoiceRequestModel> Invoices { get; set; }
        public OrderHeaderRequestModel OrderHeader {get; set;}
        public IEnumerable<VisitOrderDetailRequest> OrderDetail { get; set; }
    }
}
