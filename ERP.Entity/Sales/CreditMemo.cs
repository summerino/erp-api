using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Sales;

[Table("CreditMemo", Schema = Schema.Sales)]
[Index(nameof(TransCode))]
public class CreditMemo : BaseEntityWithMark
{
    [Key]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    public short SrcTrans { get; set; }

    [Required]
    [StringLength(8)]
    public string CustCode { get; set; }

    [StringLength(17)]
    public string TransCode { get; set; }

    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(19, 6)]
    public decimal Amount { get; set; }
    
    public bool IncludeTax { get; set; }

    public int? TaxId { get; set; }

    [Precision(19, 6)]
    public decimal TaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Precision(19, 6)]
    public decimal Used { get; set; }

    [StringLength(256)]
    public string Notes { get; set; }
}

public class VwCreditMemo : BaseEntityWithMark
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public short SrcTrans { get; set; }

    public string CustCode { get; set; }

    public string TransCode { get; set; }

    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(19, 6)]
    public decimal Amount { get; set; }

    public bool IncludeTax { get; set; }

    public int? TaxId { get; set; }

    [Precision(19, 6)]
    public decimal TaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Precision(19, 6)]
    public decimal Used { get; set; }

    public string Notes { get; set; }


    [Precision(19, 6)]
    public decimal Remaining { get; set; }

    public string CustInitial { get; set; }

    public string CustName { get; set; }

    public string SrcTransName { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string Status { get; set; }
}