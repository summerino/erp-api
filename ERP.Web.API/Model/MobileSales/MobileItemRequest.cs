using ERP.Entity.MobileSales;

namespace ERP.Web.API.Model.MobileSales;

public class MobileItemRequest : MobileItemRequestHeader
{
    public IEnumerable<MobileItemRequestDetail> ItemDetails { get; set; }
}