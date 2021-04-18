using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;

namespace ERP_API.Model.Sales
{
    public class SalesDeliveryRequest : SalesDeliveryHeader
    {
        public IEnumerable<SalesDeliveryDetail> ItemDetails { get; set; }
    }
}
