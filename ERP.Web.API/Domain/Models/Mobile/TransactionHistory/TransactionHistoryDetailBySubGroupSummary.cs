namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory;

public class TransactionHistoryDetailBySubGroupSummary
{
    public long SalesId { get; set; }
    public string DetailSubGroup { get; set; }
    public decimal Total { get; set; }
    public decimal Target { get; set; }
    public decimal PercentAchieved { get; set; }
}