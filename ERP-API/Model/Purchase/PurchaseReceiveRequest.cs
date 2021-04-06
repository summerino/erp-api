using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;

namespace ERP_API.Model.Purchase
{
    public class PurchaseReceiveRequest : PurchaseReceiveHeader
    {
        public IEnumerable<PurchaseReceiveDetail> ItemDetails { get; set; }
    }
}
