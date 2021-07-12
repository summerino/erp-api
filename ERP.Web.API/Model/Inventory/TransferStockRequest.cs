using System;
using System.Collections.Generic;
using ERP.Entity.Inventory;

namespace ERP.Web.API.Model.Inventory
{
    public class TransferStockRequest : TransferStockHeader
    {
        public IEnumerable<TransferStockDetail> ItemDetails { get; set; }
    }
}
