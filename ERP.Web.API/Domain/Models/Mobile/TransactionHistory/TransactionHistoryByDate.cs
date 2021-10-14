using System;

namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory
{
    public class TransactionHistoryByDate
    {
        public long SalesId { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
    }
}
