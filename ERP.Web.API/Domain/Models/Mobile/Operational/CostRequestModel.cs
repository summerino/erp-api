using ERP.Entity.MobileSales;
using System.Collections.Generic;

namespace ERP.Web.API.Domain.Models.Mobile.Operational
{
    public class CostRequestModel : MobileCostHeader
    {
        public IEnumerable<MobileCostDetail> CostDetails { get; set; }
        public IEnumerable<MobileCostImage> CostImages { get; set; }

    }
}
