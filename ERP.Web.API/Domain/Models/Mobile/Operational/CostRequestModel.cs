using ERP.Entity.MobileSales;

namespace ERP.Web.API.Domain.Models.Mobile.Operational
{
    public class CostRequestModel : MobileCostHeader
    {
        public IEnumerable<MobileCostDetail> CostDetails { get; set; }
        public IEnumerable<MobileCostImage> CostImages { get; set; }

    }
}
