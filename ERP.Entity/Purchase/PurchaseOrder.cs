using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Purchase;

[Table("PurchaseOrderHeader", Schema = Schema.Purchasing)]
public class PurchaseOrderHeader : BaseEntityWithMarkApprovedAndViewed
{
    [Key]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Required]
    [StringLength(8)]
    public string SupCode { get; set; }

    public long RequestBy { get; set; }

    [StringLength(8)]
    public string WarehouseCode { get; set; }

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

    [Precision(5, 2)]
    public decimal FinalDiscPercent { get; set; }

    [Precision(19, 6)]
    public decimal FinalDisc { get; set; }

    public bool IncludeTax { get; set; }

    [Precision(19, 6)]
    public decimal TaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal ExemptTaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Column("DPP", TypeName = "decimal(19, 6)")]
    public decimal Dpp { get; set; }

    [StringLength(256)]
    public string Notes { get; set; }
}

public class VwPurchaseOrderHeader : BaseEntityWithMarkApprovedAndViewed
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public string SupCode { get; set; }

    public long RequestBy { get; set; }

    public string WarehouseCode { get; set; }

    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(19, 6)]
    public decimal ShipmentFee { get; set; }

    [Precision(19, 6)]
    public decimal HandlingFee { get; set; }

    [Precision(19, 6)]
    public decimal SubTotal { get; set; }

    [Precision(5, 2)]
    public decimal FinalDiscPercent { get; set; }

    [Precision(19, 6)]
    public decimal FinalDisc { get; set; }

    public bool IncludeTax { get; set; }

    [Precision(19, 6)]
    public decimal TaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal ExemptTaxAmount { get; set; }

    [Precision(19, 6)]
    public decimal Total { get; set; }

    [Precision(19, 6)]
    public decimal Dpp { get; set; }

    public string Notes { get; set; }


    public string SupName { get; set; }

    public string RequestInitial { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string ApprovedInitial { get; set; }

    public string Status { get; set; }
}

[Table("PurchaseOrderDetail", Schema = Schema.Purchasing)]
public class PurchaseOrderDetail
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

    [Precision(18, 2)]
    public decimal? QtyRcv { get; set; }

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

    [StringLength(256)]
    public string Notes { get; set; }

    [StringLength(6)]
    public string CoaInventory { get; set; }

    [Column("CoaCOGS")]
    [StringLength(6)]
    public string CoaCogs { get; set; }

    [StringLength(6)]
    public string CoaPurc { get; set; }

    [StringLength(6)]
    public string CoaPurcDisc { get; set; }

    [StringLength(6)]
    public string CoaPurcReturn { get; set; }

    public int Type { get; set; }
}

public class VwPurchaseOrderDetail
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

    [Precision(18, 2)]
    public decimal? QtyRcv { get; set; }

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

    public string Notes { get; set; }

    public string CoaInventory { get; set; }

    public string CoaCogs { get; set; }

    public string CoaPurc { get; set; }

    public string CoaPurcDisc { get; set; }

    public string CoaPurcReturn { get; set; }

    public int Type { get; set; }


    public string ItemName { get; set; }

    public int? ItemUomBuyId { get; set; }

    public string ItemUomBuyName { get; set; }

    [Precision(18, 2)]
    public decimal? ItemBuyPrice { get; set; }

    public string UomInitial { get; set; }
        
    public string UnitName { get; set; }
}