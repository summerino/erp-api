namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory
{
    public class TransactionHistoryBySubGroupSummary
    {
        public long SalesId { get; set; }
        public string GroupName { get; set; }
        public string GroupInitial { get; set; }
        public string SubGroup { get; set; }
        public decimal Total { get; set; }
    }
}
