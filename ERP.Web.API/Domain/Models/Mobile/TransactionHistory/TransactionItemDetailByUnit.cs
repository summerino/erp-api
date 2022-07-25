namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory
{
    public class TransactionItemDetailByUnit
    {
        public string TransCode { get; set; }
        public DateTime Date { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Total { get; set; }
    }
}
