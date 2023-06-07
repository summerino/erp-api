namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder
{
    public class OrderHeaderSubmitModel
    {
        public string Code { get; set; }
        public decimal Total { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ExemptTaxAmount { get; set; }
        public bool IncludeTax { get; set; }
        public decimal FinalDisc { get; set; }
        public decimal FinalDiscPercent { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Rate { get; set; }
        public decimal Dpp { get; set; }
        public string CurrCode { get; set; }
        public short Type { get; set; }
        public int? PaymentTermId { get; set; }
        public decimal PaidAmount { get; set; }
        public IEnumerable<VisitOrderDetailRequest> OrderDetail { get; set; }
        public IEnumerable<OrderPromoRequestModel> Promotions { get; set; }
        public IEnumerable<PaymentInvoiceRequestModel> Invoices { get; set; }
    }
}
