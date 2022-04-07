using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Accounting;

[Table("BeginningBalanceAP", Schema = Schema.Accounting)]
[Index(nameof(Code), IsUnique = true)]
public class BeginningBalanceAP : BaseEntityWithActive
{
    public long Id { get; set; }

    [Required]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Column(TypeName = "date")]
    public DateTime DueDate { get; set; }

    [Required]
    [StringLength(8)]
    public string SupCode { get; set; }

    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Precision(19, 6)]
    public decimal PaidAmount { get; set; }

    [StringLength(256)]
    public string Notes { get; set; }
}

public class VwBeginningBalanceAP : BaseEntityWithActive
{
    public long Id { get; set; }

    public string Code { get; set; }

    public DateTime Date { get; set; }

    public DateTime DueDate { get; set; }

    public string SupCode { get; set; }

    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }
        
    [Precision(19, 6)]
    public decimal PaidAmount { get; set; }

    public string Notes { get; set; }


    public string SupName { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }
}

[Table("BeginningBalanceAR", Schema = Schema.Accounting)]
[Index(nameof(Code), IsUnique = true)]
public class BeginningBalanceAR : BaseEntityWithActive
{
    public long Id { get; set; }

    [Required]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Column(TypeName = "date")]
    public DateTime DueDate { get; set; }

    [Required]
    [StringLength(8)]
    public string CustCode { get; set; }

    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Precision(19, 6)]
    public decimal PaidAmount { get; set; }

    [StringLength(256)]
    public string Notes { get; set; }
}

public class VwBeginningBalanceAR : BaseEntityWithActive
{
    public long Id { get; set; }

    public string Code { get; set; }

    public DateTime Date { get; set; }

    public DateTime DueDate { get; set; }

    public string CustCode { get; set; }

    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }
        
    [Precision(19, 6)]
    public decimal PaidAmount { get; set; }

    public string Notes { get; set; }


    public string CustName { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }
}

[Table("BeginningBalanceCreditMemo", Schema = Schema.Accounting)]
[Index(nameof(Code), IsUnique = true)]
public class BeginningBalanceCreditMemo : BaseEntityWithActive
{
    public long Id { get; set; }

    [Required]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    public short Type { get; set; }

    [Required]
    [StringLength(8)]
    public string CustCode { get; set; }

    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Precision(18, 2)]
    public decimal Used { get; set; }

    [StringLength(256)]
    public string Notes { get; set; }
}

public class VwBeginningBalanceCreditMemo : BaseEntityWithActive
{
    public long Id { get; set; }

    public string Code { get; set; }

    public DateTime Date { get; set; }

    public short Type { get; set; }

    public string TypeName { get; set; }

    public string CustCode { get; set; }

    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Precision(18, 2)]
    public decimal Used { get; set; }

    [Precision(19, 2)]
    public decimal Remaining { get; set; }

    public string Notes { get; set; }

    public string CustName { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }
}

[Table("BeginningBalanceDebitMemo", Schema = Schema.Accounting)]
[Index(nameof(Code), IsUnique = true)]
public class BeginningBalanceDebitMemo : BaseEntityWithActive
{
    public long Id { get; set; }

    [Required]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    public short Type { get; set; }

    [Required]
    [StringLength(8)]
    public string SupCode { get; set; }

    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Precision(18, 2)]
    public decimal Used { get; set; }

    [StringLength(256)]
    public string Notes { get; set; }
}

public class VwBeginningBalanceDebitMemo : BaseEntityWithActive
{
    public long Id { get; set; }

    public string Code { get; set; }

    public DateTime Date { get; set; }

    public short Type { get; set; }

    public string TypeName { get; set; }

    public string SupCode { get; set; }

    public string CurrCode { get; set; }

    [Precision(19, 6)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Precision(18, 2)]
    public decimal Used { get; set; }

    [Precision(19, 2)]
    public decimal Remaining { get; set; }

    public string Notes { get; set; }

    public string SupName { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }
}