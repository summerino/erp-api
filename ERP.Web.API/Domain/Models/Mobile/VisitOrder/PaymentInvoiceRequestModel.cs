namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder;

public class PaymentInvoiceRequestModel
{
    public string Code { get; set; }
    public string CoaCode { get; set; }
    public string TransCode { get; set; }
    public decimal Amount { get; set; }
    public string NotesFailCollect { get; set; }
    public string SrcTrans { get; set; }
}