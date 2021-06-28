using ERP_API.Domain.Entities.Expedition;
using System.Collections.Generic;

namespace ERP_API.Model.Expedition
{
    public class ExpeditionInvoiceRequest : ExpeditionInvoiceHeader
    {
        public IEnumerable<ExpeditionInvoiceDetail> Details { get; set; }
    }
}
