namespace ERP.Web.API.Domain.Models.Mobile.CustomerPromotion;

public class PromotionDetailModel
{
    public string Code { get; set; }
    public short LineNo { get; set; }
    public short ApplyTo { get; set; }
    public int? ItemId { get; set; }
    public short PromoType { get; set; }
    public bool IsPercentage { get; set; }
    public decimal ValuePercentage { get; set; }
    public decimal ValueAmount { get; set; }
    public bool IsPromoWithBudget { get; set; }
    public decimal BudgetMaximumValue { get; set; }
    public short OverBudgetAction { get; set; }
    public string SubGroup1 { get; set; }
    public string SubGroup2 { get; set; }
    public string SubGroup3 { get; set; }
    public string SubGroup4 { get; set; }
    public string SubGroup5 { get; set; }
}