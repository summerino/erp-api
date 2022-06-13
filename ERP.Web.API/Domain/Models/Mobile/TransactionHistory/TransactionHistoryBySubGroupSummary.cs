namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory;

public class TransactionHistoryBySubGroupSummary
{
    public DateTime Date { get; set; }
    public long SalesId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public string GroupInitial { get; set; }
    public int SubGroupId { get; set; }
    public string SubGroup { get; set; }
    public decimal Total { get; set; }
}