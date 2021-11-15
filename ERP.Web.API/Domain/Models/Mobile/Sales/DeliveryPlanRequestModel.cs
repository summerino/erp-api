using ERP.Entity.MobileWarehouse;

namespace ERP.Web.API.Domain.Models.Mobile.Sales
{
    public class DeliveryPlanRequestModel : MobileDeliveryItemHeader
    {
        public IEnumerable<MobileDeliveryItemDetail> DPDetails { get; set; }
    }
}
