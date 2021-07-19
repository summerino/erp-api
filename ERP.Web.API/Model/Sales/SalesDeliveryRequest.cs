using System;
using System.Collections.Generic;
using ERP.Entity.Sales;

namespace ERP.Web.API.Model.Sales
{
    public class SalesDeliveryRequest : SalesDeliveryHeader
    {
        public IEnumerable<SalesDeliveryDetailRequest> ItemDetails { get; set; }

        public DateTime InvDate { get; set; }

        public DateTime InvDueDate { get; set; }

        public bool IsSoInv { get; set; }

        public DateTime? OriginalDate { get; set; }
    }

    public class SalesDeliveryDetailRequest : SalesDeliveryDetail
    {
        public IEnumerable<SalesDeliveryDetailFreeGood> FreeItemDetails { get; set; }
    }
}
