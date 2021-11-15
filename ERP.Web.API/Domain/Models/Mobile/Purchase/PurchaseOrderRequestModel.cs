using ERP.Entity.MobileWarehouse;

namespace ERP.Web.API.Domain.Models.Mobile.Purchase
{
    public class PurchaseOrderRequestModel : MobileReceiveItemHeader
    {
        public IEnumerable<MobileReceiveItemDetail> PODetails { get; set; }
    }
}
