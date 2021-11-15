using ERP.Entity.AssetManagement;

namespace ERP.Web.API.Model.AssetManagement
{
    public class FixedAssetRequest : FixedAsset
    {
        public DateTime? OriginalPurchaseDate { get; set; }

        public DateTime? OriginalStartDepreciateOn { get; set; }
    }
}
