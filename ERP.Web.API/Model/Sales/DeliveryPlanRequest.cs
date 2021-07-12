using ERP.Entity.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model.Sales
{
    public class DeliveryPlanRequest : DeliveryPlanHeader
    {
        public IEnumerable<DeliveryPlanDetailRequest> ItemDetails { get; set; }
    }

    public class DeliveryPlanDetailRequest : DeliveryPlanDetail
    {
        public IEnumerable<DeliveryPlanUndeliveredItem> UndeliveredItems { get; set; }
    }
}
