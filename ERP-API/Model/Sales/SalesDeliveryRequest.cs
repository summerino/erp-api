using System;
using System.Collections.Generic;
using ERP_API.Domain.Entities.Sales;

namespace ERP_API.Model.Sales
{
    public class SalesDeliveryRequest : SalesDeliveryHeader
    {
        public IEnumerable<SalesDeliveryDetailRequest> ItemDetails { get; set; }

        public DateTime InvDate { get; set; }

        public DateTime InvDueDate { get; set; }

        public bool IsSoInv { get; set; }
    }

    public class SalesDeliveryDetailRequest : SalesDeliveryDetail
    {
        public IEnumerable<SalesDeliveryDetailFreeGood> FreeItemDetails { get; set; }
    }
}
