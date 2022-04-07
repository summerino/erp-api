using ERP.Entity.MobileWarehouse;

namespace ERP.Web.API.Model.MobileWarehouse;

public class MobileDeliveryItemRequest : MobileDeliveryItemHeader
{
    public IEnumerable<MobileDeliveryItemDetail> ItemDetails { get; set; }
}