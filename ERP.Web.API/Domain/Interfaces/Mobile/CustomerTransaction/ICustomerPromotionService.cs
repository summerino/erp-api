using ERP.Common.Models;
using ERP.Web.API.Domain.Models.Mobile.CustomerPromotion;

namespace ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;

public interface ICustomerPromotionService
{
    IEnumerable<PromotionHeaderModel> GetDataPromotion(string search, string custCode);
    IEnumerable<PromotionDetailModel> GetDataPromotionDetail(string code);
}