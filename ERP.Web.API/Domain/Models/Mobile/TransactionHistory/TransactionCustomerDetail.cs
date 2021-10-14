using System;

namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory
{
    public class TransactionCustomerDetail
    {
        public long SalesId { get; set; }
        public DateTime Date { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Total { get; set; }
    }
}
