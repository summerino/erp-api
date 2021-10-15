using ERP.Entity.MobileWarehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model.MobileWarehouse
{
    public class MobileDeliveryItemRequest : MobileDeliveryItemHeader
    {
        public IEnumerable<MobileDeliveryItemDetail> ItemDetails { get; set; }
    }
}
