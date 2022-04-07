using ERP.Entity.Sales;

namespace ERP.Web.API.Model.Sales;

public class SalesOrderRequest : SalesOrderHeader
{
    public IEnumerable<SalesOrderDetailRequest> ItemDetails { get; set; }

    public IEnumerable<SalesOrderPromoRequest> ListPromo { get; set; }

    public DateTime DlvDate { get; set; }

    public bool IsSoDlv { get; set; }

    public DateTime InvDate { get; set; }

    public DateTime InvDueDate { get; set; }

    public bool IsSoInv { get; set; }

    public DateTime? OriginalDate { get; set; }

    public int CustTypeId { get; set; }
}

public class SalesOrderDetailRequest : SalesOrderDetail
{
    public IEnumerable<SalesOrderDetailFreeGood> FreeItemDetails { get; set; }

    public IEnumerable<SalesOrderDetailDiscount> DiscountItemDetails { get; set; }
}

public class DetailFreeGoodData : SalesOrderDetailFreeGood 
{
    public string Initial { get; set; }

    public string Name { get; set; }

    public string UnitName { get; set; }
}

public class SalesOrderPromoRequest : PromoHeader
{
    public bool UsePromo { get; set; }
}