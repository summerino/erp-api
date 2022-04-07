using ERP.Entity.MobileSales;

namespace ERP.Web.API.Model.MobileSales;

public class MobileCostRequest : MobileCostHeader
{
    public IEnumerable<MobileCostDetail> ItemDetails { get; set; }
    public IEnumerable<MobileCostImage> ImageDetails { get; set; }
}