namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory
{
    public class TransactionIndividual
    {
        public DateTime Date { get; set; }
        public DateTime? Time { get; set; }
        public decimal Total { get; set; }
    }
}
