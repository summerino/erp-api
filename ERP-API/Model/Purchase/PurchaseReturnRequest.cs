using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;

namespace ERP_API.Model.Purchase
{
    public class PurchaseReturnRequest : PurchaseReturnHeader
    {
        public IEnumerable<PurchaseReturnDetail> ItemDetails { get; set; }
    }
}
