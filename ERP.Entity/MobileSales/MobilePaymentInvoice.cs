using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales;

[Table("MobilePaymentInvoice", Schema = Schema.MobileSales)]
[Index(nameof(CustCode))]
[Index(nameof(CoaCode))]
[Index(nameof(TransCode))]
public class MobilePaymentInvoice : BaseEntityWithMarkApprovedAndRejected
{
    [Key]
    [StringLength(17)]
    public string Code { get; set; }

    [Required]
    [StringLength(17)]
    public string VisitLogCode { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    public long SalesmanId { get; set; }

    [Required]
    [StringLength(17)]
    public string CustCode { get; set; }

    [Required]
    [StringLength(6)]
    public string CoaCode { get; set; }

    [Required]
    [StringLength(17)]
    public string TransCode { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [StringLength(256)]
    public string NotesFailCollect { get; set; }

    [Required]
    [StringLength(5)]
    public string SrcTrans { get; set; }
}

public class VwMobilePaymentInvoice : BaseEntityWithMarkApprovedAndRejected
{
    public string Code { get; set; }

    public string VisitLogCode { get; set; }

    public DateTime Date { get; set; }

    public long SalesmanId { get; set; }

    public string CustCode { get; set; }

    public string CoaCode { get; set; }

    public string TransCode { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    public string NotesFailCollect { get; set; }

    public string SrcTrans { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string ApprovedInitial { get; set; }

    public string RejectedInitial { get; set; }

    public string SalesmanInitial { get; set; }

    public string SalesmanName { get; set; }

    public string CustomerInitial { get; set; }

    public string CustomerName { get; set; }

    public string CoaName { get; set; }

    public string Status { get; set; }
}