using ERP.Entity.Inventory;
using System.Collections.Generic;
using ERP.Entity.Inventory;

namespace ERP_API.Model.Inventory
{
    public class AdjustmentRequest : AdjustmentHeader
    {
        public IEnumerable<AdjustmentDetail> ItemDetails { get; set; }
    }
}
