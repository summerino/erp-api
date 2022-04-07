using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileCustomer;

[Table("MobileOrderHeader", Schema = Schema.MobileCustomer)]
[Index(nameof(SalesOrderCode), Name = "IX_MobileCustomer_MobileOrderHeader_SalesOrderCode")]
[Index(nameof(CustCode), Name = "IX_MobileCustomer_MobileOrderHeader_CustCode")]
public class MobileOrderHeader : BaseEntityWithMarkApprovedAndRejected
{
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [StringLength(17)]
    public string SalesOrderCode { get; set; }

    [Required]
    [StringLength(8)]
    public string CustCode { get; set; }
        
    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal SubTotal { get; set; }

    public bool IncludeTax { get; set; }

    [Precision(18, 2)]
    public decimal TaxAmount { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }

    [Column("DPP", TypeName = "decimal(18, 2)")]
    public decimal Dpp { get; set; }
}

public class VwMobileOrderHeader : BaseEntityWithMarkApprovedAndRejected
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public string SalesOrderCode { get; set; }

    public string CustCode { get; set; }

    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal SubTotal { get; set; }

    public bool IncludeTax { get; set; }

    [Precision(18, 2)]
    public decimal TaxAmount { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }

    [Precision(18, 2)]
    public decimal Dpp { get; set; }


    public string CustName { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string ApprovedInitial { get; set; }

    public string RejectedInitial { get; set; }

    public string Status { get; set; }
}

[Table("MobileOrderDetail", Schema = Schema.MobileCustomer)]
[Index(nameof(Code), Name = "IX_MobileCustomer_MobileOrderDetail_Code")]
[Index(nameof(ItemId), Name = "IX_MobileCustomer_MobileOrderDetail_ItemId")]
[Index(nameof(UomId), Name = "IX_MobileCustomer_MobileOrderDetail_UomId")]
[Index(nameof(UnitId), Name = "IX_MobileCustomer_MobileOrderDetail_UnitId")]
[Index(nameof(TaxId), Name = "IX_MobileCustomer_MobileOrderDetail_TaxId ")]
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

    public int? TaxId { get; set; }

    [Precision(18, 2)]
    public decimal TaxAmount { get; set; }

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

    public int? TaxId { get; set; }

    [Precision(18, 2)]
    public decimal TaxAmount { get; set; }

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

[Table("MobileOrderDetailDiscount", Schema = Schema.MobileCustomer)]
[Index(nameof(Code), Name = "IX_MobileCustomer_MobileOrderDetailDiscount_Code")]
[Index(nameof(OrderDetailId), Name = "IX_MobileCustomer_MobileOrderDetailDiscount_OrderDetailId")]
[Index(nameof(PromoCode), Name = "IX_MobileCustomer_MobileOrderDetailDiscount_PromoCode")]
[Index(nameof(PromoDetailId), Name = "IX_MobileCustomer_MobileOrderDetailDiscount_PromoDetailId")]
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

[Table("MobileOrderDetailFreeGood", Schema = Schema.MobileCustomer)]
[Index(nameof(Code), Name = "IX_MobileCustomer_MobileOrderDetailFreeGood_Code")]
[Index(nameof(OrderDetailId), Name = "IX_MobileCustomer_MobileOrderDetailFreeGood_OrderDetailId")]
[Index(nameof(PromoCode), Name = "IX_MobileCustomer_MobileOrderDetailFreeGood_PromoCode")]
[Index(nameof(ItemId), Name = "IX_MobileCustomer_MobileOrderDetailFreeGood_ItemId")]
[Index(nameof(UomId), Name = "IX_MobileCustomer_MobileOrderDetailFreeGood_UomId")]
[Index(nameof(UnitId), Name = "IX_MobileCustomer_MobileOrderDetailFreeGood_UnitId")]
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