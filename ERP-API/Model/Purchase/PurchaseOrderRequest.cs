using System;
using System.Collections.Generic;
using ERP.Entity.Purchase;

namespace ERP_API.Model.Purchase
{
    public class PurchaseOrderRequest : PurchaseOrderHeader
    {
        public IEnumerable<PurchaseOrderDetail> ItemDetails { get; set; }

        public string RcvRefNo { get; set; }

        public DateTime RcvDate { get; set; }

        public bool IsPoRcv { get; set; }

        public string InvRefNo { get; set; }

        public DateTime InvDate { get; set; }

        public DateTime InvDueDate { get; set; }

        public bool IsPoInv { get; set; }
    }
}
