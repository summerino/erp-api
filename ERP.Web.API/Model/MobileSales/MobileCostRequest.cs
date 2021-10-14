using ERP.Entity.MobileSales;
using System.Collections.Generic;

namespace ERP.Web.API.Model.MobileSales
{
    public class MobileCostRequest : MobileCostHeader
    {
        public IEnumerable<MobileCostDetail> ItemDetails { get; set; }
        public IEnumerable<MobileCostImage> ImageDetails { get; set; }
    }
}
