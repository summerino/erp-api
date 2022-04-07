using ERP.Entity.MobileSales;

namespace ERP.Web.API.Domain.Models.Mobile.ItemRequest;

public class ItemRequestModel: MobileItemRequestHeader
{
    public IEnumerable<MobileItemRequestDetail> ItemDetails { get; set; }
}