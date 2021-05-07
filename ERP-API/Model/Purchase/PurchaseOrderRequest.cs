using System;
using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;

namespace ERP_API.Model.Purchase
{
    public class PurchaseOrderRequest : PurchaseOrderHeader
    {
        public IEnumerable<PurchaseOrderDetail> ItemDetails { get; set; }

        public string RcvRefNo { get; set; }

        public DateTime RcvDate { get; set; }

        public bool IsPoRcv { get; set; }
    }
}
