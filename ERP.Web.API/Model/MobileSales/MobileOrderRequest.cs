using ERP.Entity.MobileSales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Model.MobileSales
{
    public class MobileOrderRequest : MobileOrderHeader
    {
        public IEnumerable<MobileOrderDetailRequest> ItemDetails { get; set; }
    }

    public class MobileOrderDetailRequest : MobileOrderDetail
    {
        public IEnumerable<MobileOrderDetailFreeGood> FreeItemDetails { get; set; }

        public IEnumerable<MobileOrderDetailDiscount> DiscountItemDetails { get; set; }
    }

    public class MobileDetailFreeGoodData : MobileOrderDetailFreeGood
    {
        public string Initial { get; set; }

        public string Name { get; set; }

        public string UnitName { get; set; }
    }
}
