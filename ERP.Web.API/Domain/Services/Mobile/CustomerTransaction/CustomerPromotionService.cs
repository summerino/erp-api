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

        public IEnumerable<PromotionHeaderModel> GetDataPromotion(string search, string custCode)
        {
            var custTypeId = getCustTypeId(custCode);

            var data_1 = from header in Db.VwPromoHeaders
                         where header.Mark.Equals("A") && header.ApplyTo.Equals(1)
                         select new PromotionHeaderModel
                         {
                             Code = header.Code,
                             Name = header.Name,
                             EndDate = header.EndDate,
                             StartDate = header.StartDate,
                             ApplyTo = header.ApplyTo,
                             Content = header.Content,
                         };

            var data_2 = from header in Db.VwPromoHeaders
                         join subject in Db.PromoSubjects on header.Code equals subject.Code
                         where header.Mark.Equals("A") && header.ApplyTo.Equals(2) && subject.CustCode.Equals(custCode)
                         select new PromotionHeaderModel
                         {
                             Code = header.Code,
                             Name = header.Name,
                             EndDate = header.EndDate,
                             StartDate = header.StartDate,
                             ApplyTo = header.ApplyTo,
                             Content = header.Content,
                         };

            var data_3 = from header in Db.VwPromoHeaders
                         join subject in Db.PromoSubjects on header.Code equals subject.Code
                         join cust in Db.Customers on subject.CustCode equals cust.Code
                         where header.Mark.Equals("A") && header.ApplyTo.Equals(3) && cust.TypeId.Equals(custTypeId)
                         select new PromotionHeaderModel
                         {
                             Code = header.Code,
                             Name = header.Name,
                             EndDate = header.EndDate,
                             StartDate = header.StartDate,
                             ApplyTo = header.ApplyTo,
                             Content = header.Content,
                         };

            var data = (data_1.Union(data_2)).Union(data_3);

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
            }

            return data;
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

        private int getCustTypeId(string custCode)
        {
            return Db.Customers.FirstOrDefault(t => t.Code == custCode).TypeId;
        }
    }
}
