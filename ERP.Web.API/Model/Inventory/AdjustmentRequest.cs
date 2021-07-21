using ERP.Entity.Inventory;
using System.Collections.Generic;
using ERP.Entity.Inventory;
using System;

namespace ERP.Web.API.Model.Inventory
{
    public class AdjustmentRequest : AdjustmentHeader
    {
        public IEnumerable<AdjustmentDetail> ItemDetails { get; set; }

        public DateTime? OriginalDate { get; set; }
    }
}
