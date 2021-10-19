using ERP.Entity.MobileWarehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model.MobileWarehouse
{
    public class MobileTransferStockRequest : MobileTransferStockHeader
    {
        public IEnumerable<MobileTransferStockDetail> ItemDetails { get; set; }
    }
}
