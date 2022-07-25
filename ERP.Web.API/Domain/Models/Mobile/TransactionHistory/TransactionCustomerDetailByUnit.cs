namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory
{
    public class TransactionCustomerDetailByUnit
    {
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Total { get; set; }
    }
}
