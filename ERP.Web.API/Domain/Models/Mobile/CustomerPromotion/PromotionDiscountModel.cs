namespace ERP.Web.API.Domain.Models.Mobile.CustomerPromotion
{
    public class PromotionDiscountModel
    {
        public string PromoCode { get; set; }
        public long PromoDetailId { get; set; }
        public string Name { get; set; }
        public bool IsPercentage { get; set; }
        public decimal Value { get; set; }
        public bool IsPromoWithBudget { get; set; }
        public decimal BudgetMaximumValue { get; set; }
        public int OverBudgetAction { get; set; }

    }
}
