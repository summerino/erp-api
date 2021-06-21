using System;
using System.Collections.Generic;
using ERP_API.Domain.Entities.Purchase;

namespace ERP_API.Model.Purchase
{
    public class PurchaseReceiveRequest : PurchaseReceiveHeader
    {
        public IEnumerable<PurchaseReceiveDetail> ItemDetails { get; set; }

        public string InvRefNo { get; set; }

        public DateTime InvDate { get; set; }

        public DateTime InvDueDate { get; set; }

        public bool IsPoInv { get; set; }
    }
}
