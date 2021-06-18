using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;

namespace ERP_API.Model.Sales
{
    public class VisitOrderRequest : VisitOrder
    {
        public IEnumerable<VisitOrderCustomer> CustomerDetails { get; set; }

        public IEnumerable<VisitOrderInvoice> InvoiceDetails { get; set; }
    }
}
