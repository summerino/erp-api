using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Purchase;

[Table("PurchaseInvoiceHeader", Schema = Schema.Purchasing)]
public class PurchaseInvoiceHeader : BaseEntityWithMarkApprovedAndViewed
{
    [Key]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Column(TypeName = "date")]
    public DateTime DueDate { get; set; }

    [Column("POCode")]
    [StringLength(17)]
    public string PoCode { get; set; }

    [StringLength(30)]
    public string RefNo { get; set; }

    [Required]
    [StringLength(8)]
    public string SupCode { get; set; }
    
    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal PaidAmount { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [StringLength(50)]
    public string TaxInvoiceNo { get; set; }

    [Column(TypeName = "date")]
    public DateTime? TaxInvoiceDate { get; set; }

    [StringLength(256)]
    public string Notes { get; set; }
}

public class VwPurchaseInvoiceHeader : BaseEntityWithMarkApprovedAndViewed
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public DateTime DueDate { get; set; }

    public string PoCode { get; set; }

    public string RefNo { get; set; }

    public string SupCode { get; set; }

    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal PaidAmount { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Precision(23, 6)]
    public decimal Remaining { get; set; }

    public string TaxInvoiceNo { get; set; }

    public DateTime? TaxInvoiceDate { get; set; }

    public string Notes { get; set; }


    public string SupName { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string ApprovedInitial { get; set; }

    public string Status { get; set; }
}

[Table("PurchaseInvoiceDetail", Schema = Schema.Purchasing)]
public class PurchaseInvoiceDetail
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public short LineNo { get; set; }

    [Required]
    [StringLength(17)]
    public string RcvCode { get; set; }

    [Precision(19, 6)]
    public decimal ShipmentFee { get; set; }

    [Precision(19, 6)]
    public decimal HandlingFee { get; set; }

    [Precision(19, 6)]
    public decimal SubTotal { get; set; }

    [Precision(19, 6)]
    public decimal FinalDisc { get; set; }

    [Precision(19, 6)]
    public decimal TaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal ExemptTaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Column("DPP", TypeName = "decimal(19, 6)")]
    public decimal Dpp { get; set; }
}

[Table("PurchaseInvoiceDebitMemo", Schema = Schema.Purchasing)]
public class PurchaseInvoiceDebitMemo
{
    public long Id { get; set; }

    [StringLength(17)]
    public string InvCode { get; set; }

    [Required]
    [StringLength(17)]
    public string DebitMemoCode { get; set; }

    [Precision(18, 2)]
    public decimal InvAmount { get; set; }

    [Precision(18, 2)]
    public decimal DebitMemoAmount { get; set; }
}