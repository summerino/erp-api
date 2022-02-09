using ERP.Entity.Sales;

namespace ERP.Web.API.Model.Sales
{
    public class DeliveryPlanRequest : DeliveryPlanHeader
    {
        public IEnumerable<DeliveryPlanDetailRequest> ItemDetails { get; set; }

        public DateTime? OriginalDate { get; set; }
    }

    public class DeliveryPlanDetailRequest : DeliveryPlanDetail
    {
        public IEnumerable<DeliveryPlanUndeliveredItemRequest> UndeliveredItems { get; set; }
    }

    public class DeliveryPlanUndeliveredItemRequest : DeliveryPlanUndeliveredItem
    {
        public int DetailId { get; set; }
    }
}
