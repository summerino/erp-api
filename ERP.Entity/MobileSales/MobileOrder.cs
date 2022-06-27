using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales;

[Table("MobileOrderHeader", Schema = Schema.MobileSales)]
[Index(nameof(CustCode))]
public class MobileOrderHeader : BaseEntityWithMarkApprovedAndRejected
{
    [Key]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Required]
    [StringLength(17)]
    public string VisitLogCode { get; set; }

    [StringLength(17)]
    public string SalesOrderCode { get; set; }

    public short Type { get; set; }

    [Required]
    [StringLength(17)]
    public string CustCode { get; set; }

    public long SalesBy { get; set; }

    public int? PaymentTermId { get; set; }

    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal SubTotal { get; set; }

    [Precision(5, 2)]
    public decimal FinalDiscPercent { get; set; }

    [Precision(18, 2)]
    public decimal FinalDisc { get; set; }

    public bool IncludeTax { get; set; }

    [Precision(18, 2)]
    public decimal TaxAmount { get; set; }

    [Precision(18, 2)]
    public decimal ExemptTaxAmount { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }

    [Column("DPP", TypeName = "decimal(18, 2)")]
    public decimal Dpp { get; set; }

    [Precision(18, 2)]
    public decimal PaidAmount { get; set; }
}

public class VwMobileOrderHeader : BaseEntityWithMarkApprovedAndRejected
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public string VisitLogCode { get; set; }

    public string SalesOrderCode { get; set; }

    public short Type { get; set; }

    public string CustCode { get; set; }

    public long SalesBy { get; set; }

    public int? PaymentTermId { get; set; }

    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal SubTotal { get; set; }

    [Precision(5, 2)]
    public decimal FinalDiscPercent { get; set; }

    [Precision(18, 2)]
    public decimal FinalDisc { get; set; }

    public bool IncludeTax { get; set; }

    [Precision(18, 2)]
    public decimal TaxAmount { get; set; }

    [Precision(18, 2)]
    public decimal ExemptTaxAmount { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }

    [Precision(18, 2)]
    public decimal Dpp { get; set; }

    [Precision(18, 2)]
    public decimal PaidAmount { get; set; }

    public string CustName { get; set; }

    public string SalesInitial { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string ApprovedInitial { get; set; }

    public string RejectedInitial { get; set; }

    public string Status { get; set; }
}

[Table("MobileOrderDetail", Schema = Schema.MobileSales)]
public class MobileOrderDetail
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public short LineNo { get; set; }

    public int ItemId { get; set; }

    public int UomId { get; set; }

    public int UnitId { get; set; }
        
    [Precision(18, 2)]
    public decimal Qty { get; set; }

    [Precision(19, 6)]
    public decimal UnitPrice { get; set; }

    [Precision(19, 6)]
    public decimal Disc { get; set; }

    [Precision(19, 6)]
    public decimal FinalDiscHeader { get; set; }

    public int? TaxId { get; set; }

    [Precision(19, 6)]
    public decimal TaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal ExemptTaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal NettPrice { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Column("DPP", TypeName = "decimal(19, 6)")]
    public decimal Dpp { get; set; }
}

public class VwMobileOrderDetail
{
    public long Id { get; set; }

    public string Code { get; set; }

    public short LineNo { get; set; }

    public int ItemId { get; set; }

    public int UomId { get; set; }

    public int UnitId { get; set; }

    [Precision(18, 2)]
    public decimal Qty { get; set; }

    [Precision(19, 6)]
    public decimal UnitPrice { get; set; }

    [Precision(19, 6)]
    public decimal Disc { get; set; }

    [Precision(19, 6)]
    public decimal FinalDiscHeader { get; set; }

    public int? TaxId { get; set; }

    [Precision(19, 6)]
    public decimal TaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal ExemptTaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal NettPrice { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Precision(19, 6)]
    public decimal Dpp { get; set; }

    public string ItemInitial { get; set; }

    public string ItemName { get; set; }

    public int? ItemUomSellId { get; set; }

    public string ItemUomSellName { get; set; }

    [Precision(18, 2)]
    public decimal? ItemSellPrice { get; set; }

    public string UomInitial { get; set; }

    public string UnitName { get; set; }
}

[Table("MobileOrderDetailDiscount", Schema = Schema.MobileSales)]
public class MobileOrderDetailDiscount
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public long OrderDetailId { get; set; }

    public short LineNo { get; set; }

    [StringLength(17)]
    public string PromoCode { get; set; }

    public long? PromoDetailId { get; set; }

    [StringLength(50)]
    public string Name { get; set; }

    public bool IsPercentage { get; set; }

    [Precision(18, 2)]
    public decimal Value { get; set; }

    [Precision(19, 6)]
    public decimal Amount { get; set; }
}

[Table("MobileOrderDetailFreeGood", Schema = Schema.MobileSales)]
public class MobileOrderDetailFreeGood
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public long OrderDetailId { get; set; }

    public short LineNo { get; set; }

    [Required]
    [StringLength(17)]
    public string PromoCode { get; set; }

    public int ItemId { get; set; }

    public int UomId { get; set; }

    public int UnitId { get; set; }

    [Precision(19, 6)]
    public decimal Qty { get; set; }

    [Precision(19, 6)]
    public decimal UnitPrice { get; set; }
}