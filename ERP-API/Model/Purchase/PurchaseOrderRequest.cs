using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;

namespace ERP_API.Model.Purchase
{
    public class PurchaseOrderRequest : PurchaseOrderHeader
    {
        public IEnumerable<PurchaseOrderDetail> ItemDetails { get; set; }
    }
}
