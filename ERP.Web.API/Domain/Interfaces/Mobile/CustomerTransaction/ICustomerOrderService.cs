using ERP.Entity.General;
using ERP.Web.API.Domain.Models.Mobile.CustomerPromotion;

namespace ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction
{
    public interface ICustomerOrderService
    {
        Tax GetTransactionTax(int taxId);
        IEnumerable<PromotionDiscountModel> GetPromotionDiscount(int itemId, int itemCatId, decimal qty,int unitId,string custCode);
        IEnumerable<PromotionFreeGoodModel> GetPromotionFreeGood(int itemId, int itemCatId, decimal qty, int unitId, string custCode);
        IEnumerable<PromotionDiscountModel> GetPromotionInvoice(decimal amount, string custCode);
        bool GetParams();
    }
}
