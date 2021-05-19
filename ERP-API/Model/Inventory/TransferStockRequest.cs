using System;
using System.Collections.Generic;
using ERP_API.Domain.Entities.Inventory;

namespace ERP_API.Model.Inventory
{
    public class TransferStockRequest : TransferStockHeader
    {
        public IEnumerable<TransferStockDetail> ItemDetails { get; set; }
    }
}
