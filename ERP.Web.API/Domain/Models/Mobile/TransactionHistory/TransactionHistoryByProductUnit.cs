namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory
{
    public class TransactionHistoryByProductUnit
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Total { get; set; }
    }
}
