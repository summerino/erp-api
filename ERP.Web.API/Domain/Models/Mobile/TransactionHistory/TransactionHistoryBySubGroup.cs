namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory;

public class TransactionHistoryBySubGroup
{
    public long SalesId { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
}