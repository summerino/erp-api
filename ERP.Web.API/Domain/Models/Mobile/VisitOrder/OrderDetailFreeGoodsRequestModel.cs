namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder
{
    public class OrderDetailFreeGoodsRequestModel
    {
        public string PromoCode { get; set; }
        public int ItemId { get; set; }
        public int UomId { get; set; }
        public int UnitId { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
