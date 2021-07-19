using ERP.Entity.AssetManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model.AssetManagement
{
    public class FixedAssetRequest : FixedAsset
    {
        public DateTime? OriginalPurchaseDate { get; set; }

        public DateTime? OriginalStartDepreciateOn { get; set; }
    }
}
