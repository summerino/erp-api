using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.General;
using ERP.Entity.MobileCustomer;
using ERP.Web.API.Domain.Models.Mobile.CustomerOrder;
using ERP.Web.API.Domain.Models.Mobile.CustomerPromotion;
using ERP.Web.API.Domain.Models.Mobile.VisitOrder;

namespace ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction
{
    public interface ICustomerOrderService
    {
        Tax GetTransactionTax(int taxId);
        IEnumerable<PromotionDiscountModel> GetPromotionDiscount(int itemId, int itemCatId, decimal qty,int unitId,string custCode);
        IEnumerable<PromotionFreeGoodModel> GetPromotionFreeGood(int itemId, int itemCatId, decimal qty, int unitId, string custCode);
        IEnumerable<PromotionDiscountModel> GetPromotionInvoice(decimal amount, string custCode);
        bool GetParams();
        DataSourceResult GetCustomerOrderHeader(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? date, string custCode);
        IEnumerable<OrderCustomerDetailModel> GetCustomerOrderDetail(string orderId);
        SaveResult InsertOrderCustomer(MobileOrderHeader header, IEnumerable<VisitOrderDetailRequest> details);


    }
}
