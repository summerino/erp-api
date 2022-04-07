namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder;

public class SalesInvoiceModel
{
    public string Code { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public string SoCode { get; set; }
    public string CustCode { get; set; }
    public int? PaymentTermId { get; set; }
    public string PaymentTermName { get; set; }
    public string CurrCode { get; set; }
    public string CurrName { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Total { get; set; }
    public string Notes { get; set; }
    public DateTime UpdatedDate { get; set; }
}