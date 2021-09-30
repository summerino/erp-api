using ERP.Entity.MobileSales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model.MobileSales
{
    public class MobileItemRequest : MobileItemRequestHeader
    {
        public IEnumerable<MobileItemRequestDetail> ItemDetails { get; set; }
    }
}
