using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Sales;

[Table("SalesInvoiceHeader", Schema = Schema.Sales)]
public class SalesInvoiceHeader : BaseEntityWithMarkApprovedAndViewed
{
    [Key]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Column(TypeName = "date")]
    public DateTime DueDate { get; set; }

    [Column("SOCode")]
    [StringLength(17)]
    public string SoCode { get; set; }

    [Required]
    [StringLength(8)]
    public string CustCode { get; set; }

    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal PaidAmount { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }

    [StringLength(256)]
    public string Notes { get; set; }

    public bool FromDirectInvoice { get; set; }
}

public class VwSalesInvoiceHeader : BaseEntityWithMarkApprovedAndViewed
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public DateTime DueDate { get; set; }

    public string SoCode { get; set; }

    public string CustCode { get; set; }

    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal PaidAmount { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }

    [Precision(23, 6)]
    public decimal Remaining { get; set; }

    public string Notes { get; set; }

    public bool FromDirectInvoice { get; set; }


    public string CustName { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string ApprovedInitial { get; set; }

    public string Status { get; set; }

    public string CustAddress { get; set; }

    public string CustArea { get; set; }
}

[Table("SalesInvoiceDetail", Schema = Schema.Sales)]
public class SalesInvoiceDetail
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public short LineNo { get; set; }

    [Required]
    [Column("DOCode")]
    [StringLength(17)]
    public string DoCode { get; set; }

    [Precision(18, 2)]
    public decimal ShipmentFee { get; set; }

    [Precision(18, 2)]
    public decimal HandlingFee { get; set; }

    [Precision(18, 2)]
    public decimal SubTotal { get; set; }

    [Precision(18, 2)]
    public decimal FinalDisc { get; set; }

    [Precision(18, 2)]
    public decimal TaxAmount { get; set; }

    [Precision(18, 2)]
    public decimal ExemptTaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Column("DPP", TypeName = "decimal(19, 6)")]
    public decimal Dpp { get; set; }
}

[Table("SalesInvoiceCreditMemo", Schema = Schema.Sales)]
public class SalesInvoiceCreditMemo
{
    public long Id { get; set; }

    [StringLength(17)]
    public string InvCode { get; set; }

    [Required]
    [StringLength(17)]
    public string CreditMemoCode { get; set; }

    [Precision(18, 2)]
    public decimal InvAmount { get; set; }

    [Precision(18, 2)]
    public decimal CreditMemoAmount { get; set; }
}