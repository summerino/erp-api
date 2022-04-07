using ERP.Entity.Sales;

namespace ERP.Web.API.Model.Sales;

public class VisitOrderRequest : VisitOrder
{
    public IEnumerable<VisitOrderCustomer> CustomerDetails { get; set; }

    public IEnumerable<VisitOrderInvoice> InvoiceDetails { get; set; }

    public DateTime? OriginalDate { get; set; }
}