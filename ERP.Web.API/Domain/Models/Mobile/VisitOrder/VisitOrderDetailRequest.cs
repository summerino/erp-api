namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder;

public class VisitOrderDetailRequest
{
    public int ItemId { get; set; }
    public int UomId { get; set; }
    public int UnitId { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Disc { get; set; }
    public int? TaxId { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ExemptTaxAmount { get; set; }
    public decimal NettPrice { get; set; }
    public decimal Total { get; set; }
    public decimal Dpp { get; set; }
    public IEnumerable<OrderDetailDiscountRequestModel> Discounts { get; set; }
    public IEnumerable<OrderDetailFreeGoodsRequestModel> FreeGoods { get; set; }
}