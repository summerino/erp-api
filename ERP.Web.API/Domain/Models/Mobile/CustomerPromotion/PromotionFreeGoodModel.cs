namespace ERP.Web.API.Domain.Models.Mobile.CustomerPromotion
{
    public class PromotionFreeGoodModel
    {
        public string PromoCode { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int UomId { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
