using ERP.Entity.MobileSales;

namespace ERP.Web.API.Model.MobileSales;

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