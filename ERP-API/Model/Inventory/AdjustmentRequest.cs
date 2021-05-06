using ERP_API.Domain.Entities.Inventory;
using System.Collections.Generic;

namespace ERP_API.Model.Inventory
{
    public class AdjustmentRequest : AdjustmentHeader
    {
        public IEnumerable<AdjustmentDetail> ItemDetails { get; set; }
    }
}
