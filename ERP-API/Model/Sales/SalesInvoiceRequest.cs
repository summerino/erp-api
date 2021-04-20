using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;

namespace ERP_API.Model.Sales
{
    public class SalesInvoiceRequest : SalesInvoiceHeader
    {
        public IEnumerable<SalesInvoiceDetail> Details { get; set; }
    }
}
