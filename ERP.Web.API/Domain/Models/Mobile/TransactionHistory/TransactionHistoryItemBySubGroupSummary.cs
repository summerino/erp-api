namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory;

public class TransactionHistoryItemBySubGroupSummary
{
    public long SalesId { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public string Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ExemptTaxAmount { get; set; }
    public decimal TotalReal { get; set; }
    public decimal TotalMobile { get; set; }
    public decimal Total { get; set; }
}