using ERP.Entity.MobileSales;

namespace ERP.Web.API.Model.MobileSales
{
    public class MobilePaymentInvoiceApproveRequest
    {
        public List<MobilePaymentInvoice> Data { get; set; }

        public string Notes { get; set; }
    }
}
