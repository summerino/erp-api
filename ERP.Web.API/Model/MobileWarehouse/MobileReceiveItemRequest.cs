using ERP.Entity.MobileWarehouse;

namespace ERP.Web.API.Model.MobileWarehouse
{
    public class MobileReceiveItemRequest : MobileReceiveItemHeader
    {
        public IEnumerable<MobileReceiveItemDetail> ItemDetails { get; set; }
    }
}
