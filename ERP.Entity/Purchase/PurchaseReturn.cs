using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Purchase;

[Table("PurchaseReturnHeader", Schema = Schema.Purchasing)]
public class PurchaseReturnHeader : BaseEntityWithMarkApprovedAndViewed
{
    [Key]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [StringLength(17)]
    public string RcvCode { get; set; }

    [StringLength(30)]
    public string RefNo { get; set; }

    public short Type { get; set; }

    [Required]
    [StringLength(8)]
    public string SupCode { get; set; }
        
    public long ShippedBy { get; set; }

    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }
        
    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(19, 6)]
    public decimal ShipmentFee { get; set; }

    [Precision(19, 6)]
    public decimal HandlingFee { get; set; }

    [Precision(19, 6)]
    public decimal SubTotal { get; set; }
        
    [Precision(19, 6)]
    public decimal FinalDisc { get; set; }

    public bool NoTax { get; set; }

    public bool IncludeTax { get; set; }

    [Precision(19, 6)]
    public decimal TaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal ExemptTaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Column("DPP", TypeName = "decimal(19, 6)")]
    public decimal Dpp { get; set; }

    [StringLength(50)]
    public string TaxInvoiceNo { get; set; }

    [Column(TypeName = "date")]
    public DateTime? TaxInvoiceDate { get; set; }

    [Required]
    [StringLength(256)]
    public string Notes { get; set; }
}

public class VwPurchaseReturnHeader : BaseEntityWithMarkApprovedAndViewed
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public string RcvCode { get; set; }

    public string RefNo { get; set; }

    public short Type { get; set; }

    public string SupCode { get; set; }

    public long ShippedBy { get; set; }

    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(19, 6)]
    public decimal ShipmentFee { get; set; }

    [Precision(19, 6)]
    public decimal HandlingFee { get; set; }

    [Precision(19, 6)]
    public decimal SubTotal { get; set; }

    [Precision(19, 6)]
    public decimal FinalDisc { get; set; }

    public bool NoTax { get; set; }

    public bool IncludeTax { get; set; }

    [Precision(19, 6)]
    public decimal TaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal ExemptTaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Precision(19, 6)]
    public decimal Dpp { get; set; }

    public string TaxInvoiceNo { get; set; }

    public DateTime? TaxInvoiceDate { get; set; }

    public string Notes { get; set; }


    public string SupName { get; set; }

    public string ShippedInitial { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string TypeName { get; set; }

    public string Status { get; set; }
}

[Table("PurchaseReturnDetail", Schema = Schema.Purchasing)]
public class PurchaseReturnDetail
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public short LineNo { get; set; }

    public long? RcvDetailId { get; set; }

    public int ItemId { get; set; }

    public int UomId { get; set; }

    public int UnitId { get; set; }

    [Precision(18, 2)]
    public decimal Qty { get; set; }

    [Precision(18, 2)]
    public decimal QtyRcv { get; set; }

    [Required]
    [StringLength(8)]
    public string WarehouseCode { get; set; }

    [StringLength(8)]
    public string WarehouseCodeIn { get; set; }

    [Precision(18, 2)]
    public decimal? Length { get; set; }

    [Precision(18, 2)]
    public decimal? Width { get; set; }

    [Precision(18, 2)]
    public decimal? Height { get; set; }

    [Precision(18, 3)]
    public decimal? Weight { get; set; }

    [StringLength(10)]
    public string DimensionMeasurement { get; set; }

    [StringLength(10)]
    public string WeightMeasurement { get; set; }

    [Precision(19, 6)]
    public decimal UnitPrice { get; set; }

    [Precision(19, 6)]
    public decimal Disc { get; set; }

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

public class VwPurchaseReturnDetail
{
    public long Id { get; set; }

    public string Code { get; set; }

    public short LineNo { get; set; }

    public long? RcvDetailId { get; set; }

    public int ItemId { get; set; }

    public int UomId { get; set; }

    public int UnitId { get; set; }

    [Precision(18, 2)]
    public decimal Qty { get; set; }

    [Precision(18, 2)]
    public decimal QtyRcv { get; set; }

    public string WarehouseCode { get; set; }

    public string WarehouseCodeIn { get; set; }

    [Precision(18, 2)]
    public decimal? Length { get; set; }

    [Precision(18, 2)]
    public decimal? Width { get; set; }

    [Precision(18, 2)]
    public decimal? Height { get; set; }

    [Precision(18, 3)]
    public decimal? Weight { get; set; }

    public string DimensionMeasurement { get; set; }

    public string WeightMeasurement { get; set; }

    [Precision(19, 6)]
    public decimal UnitPrice { get; set; }

    [Precision(19, 6)]
    public decimal Disc { get; set; }

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


    public string ItemName { get; set; }

    public int? ItemUomBuyId { get; set; }

    public string ItemUomBuyName { get; set; }

    [Precision(18, 2)]
    public decimal? ItemBuyPrice { get; set; }

    public string UomInitial { get; set; }

    public string UnitName { get; set; }
}

[Table("PurchaseReturnDetailExchDiffItem", Schema = Schema.Purchasing)]
public class PurchaseReturnDetailExchDiffItem
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public short LineNo { get; set; }

    public long? ReturnDetailId { get; set; }

    public int ItemId { get; set; }

    public int UomId { get; set; }

    public int UnitId { get; set; }

    [Precision(18, 2)]
    public decimal Qty { get; set; }

    [Precision(18, 2)]
    public decimal QtyRcv { get; set; }

    [StringLength(8)]
    public string WarehouseCode { get; set; }

    [Precision(19, 6)]
    public decimal UnitPrice { get; set; }

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

public class VwPurchaseReturnDetailExchDiffItem
{
    public long Id { get; set; }

    public string Code { get; set; }

    public short LineNo { get; set; }

    public long? ReturnDetailId { get; set; }

    public int ItemId { get; set; }

    public int UomId { get; set; }

    public int UnitId { get; set; }

    [Precision(18, 2)]
    public decimal Qty { get; set; }

    [Precision(18, 2)]
    public decimal QtyRcv { get; set; }

    public string WarehouseCode { get; set; }

    [Precision(19, 6)]
    public decimal UnitPrice { get; set; }

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

    public string UnitName { get; set; }

    public int? ItemUomBuyId { get; set; }

    public string ItemUomBuyName { get; set; }

    [Precision(18, 2)]
    public decimal? ItemBuyPrice { get; set; }
}