using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Web.API.Domain.Interfaces.Mobile.CustomerTransaction;
using ERP.Web.API.Domain.Models.Mobile.CustomerPromotion;

namespace ERP.Web.API.Domain.Services.Mobile.CustomerTransaction
{
    public class CustomerPromotionService : ICustomerPromotionService
    {
        protected TenantContext Db;

        public CustomerPromotionService(TenantContext db)
        {
            Db = db;
        }

        public DataSourceResult GetDataPromotion(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, DateTime? date, string search, string custCode)
        {
            var data = from header in Db.VwPromoHeaders
                       select new PromotionHeaderModel
                       {
                           Code = header.Code,
                           ApplyTo = header.ApplyTo,
                           ApprovedInitial = header.ApprovedInitial,
                           CoaCost = header.CoaCost,
                           CreatedInitial = header.CreatedInitial,
                           Name = header.Name,
                           EndDate = header.EndDate,
                           StartDate = header.StartDate,
                           Status = header.Status,
                           UpdatedInitial = header.UpdatedInitial,
                       };

            return data.ToDataSourceResult(skip, take, filter, sort);
        }

        public IEnumerable<PromotionDetailModel> GetDataPromotionDetail(string code)
        {
            var data = from detail in Db.PromoDetails
                       where detail.Code.Equals(code)
                       select new PromotionDetailModel
                       {
                           Code = detail.Code,
                           LineNo = detail.LineNo,
                           ApplyTo = detail.ApplyTo,
                           ItemId = detail.ItemId,
                           PromoType = detail.PromoType,
                           IsPercentage = detail.IsPercentage,
                           ValuePercentage = detail.ValuePercentage,
                           ValueAmount = detail.ValueAmount,
                           IsPromoWithBudget = detail.IsPromoWithBudget,
                           BudgetMaximumValue = detail.BudgetMaximumValue,
                           OverBudgetAction = detail.OverBudgetAction,
                           SubGroup1 = detail.SubGroup1,
                           SubGroup2 = detail.SubGroup2,
                           SubGroup3 = detail.SubGroup3,
                           SubGroup4 = detail.SubGroup4,
                           SubGroup5 = detail.SubGroup5,
                       };

            return data;
        }
    }
}
