using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales;

[Table("MobileCostHeader", Schema = Schema.MobileSales)]
public class MobileCostHeader : BaseEntityWithMarkApprovedAndRejected
{
    [Key]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [StringLength(17)]
    public string CashBankCode { get; set; }

    public long SalesmanId { get; set; }

    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }
}

public class VwMobileCostHeader : BaseEntityWithMarkApprovedAndRejected
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public string CashBankCode { get; set; }

    public long SalesmanId { get; set; }

    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string ApprovedInitial { get; set; }

    public string RejectedInitial { get; set; }

    public string SalesmanInitial { get; set; }

    public string SalesmanName { get; set; }

    public string Status { get; set; }
}

[Table("MobileCostDetail", Schema = Schema.MobileSales)]
[Index(nameof(CoaCode))]
public class MobileCostDetail
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public short LineNo { get; set; }

    [Required]
    [StringLength(6)]
    public string CoaCode { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [StringLength(256)]
    public string Notes { get; set; }
}

public class VwMobileCostDetail
{
    public long Id { get; set; }

    public string Code { get; set; }

    public short LineNo { get; set; }

    public string CoaCode { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    public string Notes { get; set; }

    
    public string CoaName { get; set; }
}

[Table("MobileCostImage", Schema = Schema.MobileSales)]
public class MobileCostImage
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public short LineNo { get; set; }

    [Required]
    public string Image { get; set; }
}