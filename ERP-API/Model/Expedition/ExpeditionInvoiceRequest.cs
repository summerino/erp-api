using System.Collections.Generic;
using ERP.Entity.Expedition;

namespace ERP_API.Model.Expedition
{
    public class ExpeditionInvoiceRequest : ExpeditionInvoiceHeader
    {
        public IEnumerable<ExpeditionInvoiceDetail> Details { get; set; }
    }
}
