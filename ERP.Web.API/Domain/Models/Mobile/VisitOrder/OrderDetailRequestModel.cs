namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder
{
    public class OrderDetailRequestModel:OrderDetailModel
    {
        public IEnumerable<PromoDiscountModel> Discounts { get; set; }
        public IEnumerable<PromoFreeGoodsModel> FreeGoods { get; set; }
    }
}
