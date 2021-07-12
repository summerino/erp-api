using System.Collections.Generic;
using ERP.Entity.Sales;

namespace ERP_API.Model.Sales
{
    public class PromoRequest : PromoHeader
    {
        public IEnumerable<PromoDetailRequest> ItemDetails { get; set; }
    }

    public class PromoDetailRequest : PromoDetail
    {
        public IEnumerable<PromoDetailTier> PromoTierList { get; set; }

        public int? SaleUnit { get; set; }

        public bool ApplyToAllUnit { get; set; }

        public int? FreeGoodItemId { get; set; }

        public string UnitFreeGood { get; set; }

        public bool IsMultiple { get; set; }
    }
}
