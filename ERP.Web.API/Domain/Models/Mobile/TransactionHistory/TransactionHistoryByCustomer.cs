namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory;

public class TransactionHistoryByCustomer
{
    public long SalesId { get; set; }
    public DateTime Date { get; set; }
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public decimal Total { get; set; }
}