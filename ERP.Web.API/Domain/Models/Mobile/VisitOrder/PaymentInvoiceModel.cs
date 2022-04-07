using System.ComponentModel.DataAnnotations;

namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder;

public class PaymentInvoiceModel
{
    public string Code { get; set; }
    [Required]
    public string VisitLogCode { get; set; }
    public DateTime Date { get; set; }
    [Required]
    public string CustCode { get; set; }
    [Required]
    public string CustName { get; set; }
    [Required]
    public string CoaCode { get; set; }
    [Required]
    public string CoaName { get; set; }
    [Required]
    public string TransCode { get; set; }
    public decimal Amount { get; set; }
    public string NotesFailCollect { get; set; }
    [Required]
    public string SrcTrans { get; set; }
    [Required]
    public DateTime UpdatedDate { get; set; }
}