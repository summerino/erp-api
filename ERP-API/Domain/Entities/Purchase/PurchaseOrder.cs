using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Purchase
{
    [Table("trPurchaseOrderHeaders", Schema = "dbo")]
    public class PurchaseOrderHeader : BaseEntityWithMark
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

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShipmentFee { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal HandlingFee { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal FinalDiscPercent { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalDisc { get; set; }

        public bool IncludeTax { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; }

        [Column("DPP", TypeName = "decimal(18, 2)")]
        public decimal Dpp { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }

        [StringLength(20)]
        public string RcvStatus { get; set; }
    }

    public class VwPurchaseOrderHeader : BaseEntityWithMark
    {
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

        public decimal Rate { get; set; }

        public decimal ShipmentFee { get; set; }

        public decimal HandlingFee { get; set; }

        public decimal SubTotal { get; set; }

        public decimal FinalDiscPercent { get; set; }

        public decimal FinalDisc { get; set; }

        public bool IncludeTax { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }

        public decimal Dpp { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }

        [StringLength(20)]
        public string RcvStatus { get; set; }


        public string SupName { get; set; }

        public string RequestInitial { get; set; }
    }

    [Table("trPurchaseOrderDetails", Schema = "dbo")]
    public class PurchaseOrderDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Length { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Width { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Height { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal? Weight { get; set; }

        [StringLength(10)]
        public string DimensionMeasurement { get; set; }

        [StringLength(10)]
        public string WeightMeasurement { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? QtyRcv { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Disc { get; set; }

        public int? TaxId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal NettPrice { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Total { get; set; }

        [Column("DPP", TypeName = "decimal(19, 6)")]
        public decimal Dpp { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }

        [StringLength(6)]
        public string CoaInventory { get; set; }

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

        public decimal Qty { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        public decimal? Weight { get; set; }

        [StringLength(10)]
        public string DimensionMeasurement { get; set; }

        [StringLength(10)]
        public string WeightMeasurement { get; set; }

        public decimal? QtyRcv { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Disc { get; set; }

        public int? TaxId { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal NettPrice { get; set; }

        public decimal Total { get; set; }

        public decimal Dpp { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }

        [StringLength(6)]
        public string CoaInventory { get; set; }

        [StringLength(6)]
        public string CoaCogs { get; set; }

        [StringLength(6)]
        public string CoaPurc { get; set; }

        [StringLength(6)]
        public string CoaPurcDisc { get; set; }

        [StringLength(6)]
        public string CoaPurcReturn { get; set; }

        public int Type { get; set; }


        public string ItemName { get; set; }

        public int? ItemUomBuyId { get; set; }

        public string ItemUomBuyName { get; set; }

        public decimal? ItemBuyPrice { get; set; }

        public string UomInitial { get; set; }
        
        public string UnitName { get; set; }
    }
}
