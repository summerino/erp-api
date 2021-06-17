using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;

namespace ERP_API.Model.Sales
{
    public class SalesDeliveryRequest : SalesDeliveryHeader
    {
        public IEnumerable<SalesDeliveryDetailRequest> ItemDetails { get; set; }
    }

    public class SalesDeliveryDetailRequest : SalesDeliveryDetail
    {
        public IEnumerable<SalesDeliveryDetailFreeGood> FreeItemDetails { get; set; }
    }
}
