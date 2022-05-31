using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Sales;

[Table("SalesDeliveryHeader", Schema = Schema.Sales)]
[Index(nameof(TransCode))]
public class SalesDeliveryHeader : BaseEntityWithMarkApprovedAndViewed
{
    [Key]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    public short SrcTrans { get; set; }

    [Required]
    [StringLength(17)]
    public string TransCode { get; set; }

    [Required]
    [StringLength(8)]
    public string CustCode { get; set; }

    [Required]
    [StringLength(8)]
    public string WarehouseCode { get; set; }

    public long ShippedBy { get; set; }

    [Required]
    [StringLength(3)]
    public string CurrCode { get; set; }
        
    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal ShipmentFee { get; set; }

    [Precision(18, 2)]
    public decimal HandlingFee { get; set; }

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

    [Precision(19, 6)]
    public decimal PaidAmount { get; set; }

    [StringLength(50)]
    public string TaxInvoiceNo { get; set; }

    [Column(TypeName = "date")]
    public DateTime? TaxInvoiceDate { get; set; }

    [StringLength(256)]
    public string Notes { get; set; }

    public bool FromDirectInvoice { get; set; }
}

public class VwSalesDeliveryHeader : BaseEntityWithMarkApprovedAndViewed
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public short SrcTrans { get; set; }

    public string TransCode { get; set; }

    public string CustCode { get; set; }

    public string WarehouseCode { get; set; }

    public long ShippedBy { get; set; }

    public string CurrCode { get; set; }

    [Precision(18, 2)]
    public decimal Rate { get; set; }

    [Precision(18, 2)]
    public decimal ShipmentFee { get; set; }

    [Precision(18, 2)]
    public decimal HandlingFee { get; set; }

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

    [Precision(19, 6)]
    public decimal PaidAmount { get; set; }

    public string TaxInvoiceNo { get; set; }

    public DateTime? TaxInvoiceDate { get; set; }

    public string Notes { get; set; }

    public bool FromDirectInvoice { get; set; }


    public string CustName { get; set; }

    public string ShippedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string Status { get; set; }

    public string CustAddress { get; set; }

    public string CustArea { get; set; }
}

[Table("SalesDeliveryDetail", Schema = Schema.Sales)]
public class SalesDeliveryDetail
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public short LineNo { get; set; }

    [Column("SODetailId")]
    public long? SoDetailId { get; set; }

    public int ItemId { get; set; }

    [Precision(18, 2)]
    public decimal Qty { get; set; }

    public int UomId { get; set; }

    public int UnitId { get; set; }

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

public class VwSalesDeliveryDetail
{
    public long Id { get; set; }

    public string Code { get; set; }

    public short LineNo { get; set; }

    public long? SoDetailId { get; set; }

    public int ItemId { get; set; }

    [Precision(18, 2)]
    public decimal Qty { get; set; }

    public int UomId { get; set; }

    public int UnitId { get; set; }

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


    [Precision(18, 2)]
    public decimal OrderQty { get; set; }

    [Precision(19, 2)]
    public decimal OutstandingQty { get; set; }

    public string ItemInitial { get; set; }

    public string ItemName { get; set; }

    public int? ItemUomSellId { get; set; }

    public string ItemUomSellName { get; set; }

    [Precision(18, 2)]
    public decimal? ItemSellPrice { get; set; }

    public string UomInitial { get; set; }

    public string UnitName { get; set; }
}

[Table("SalesDeliveryDetailFreeGood", Schema = Schema.Sales)]
public class SalesDeliveryDetailFreeGood
{
    public long Id { get; set; }

    [StringLength(17)]
    public string Code { get; set; }

    public long DlvOrderDetailId { get; set; }

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

    [StringLength(6)]
    public string CoaCode { get; set; }
}