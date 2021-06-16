using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Sales
{
    [Table("SalesOrderHeader", Schema = Schema.Sales)]
    public class SalesOrderHeader : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(8)]
        public string CustCode { get; set; }

        public long SalesBy { get; set; }

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

        public bool FromDirectInvoice { get; set; }
    }

    public class VwSalesOrderHeader : BaseEntityWithMarkAndApproved
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public string CustCode { get; set; }

        public long SalesBy { get; set; }

        public string WarehouseCode { get; set; }

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

        public string Notes { get; set; }

        public bool FromDirectInvoice { get; set; }


        public string CustName { get; set; }

        public string SalesInitial { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string Status { get; set; }
    }

    [Table("SalesOrderDetail", Schema = Schema.Sales)]
    public class SalesOrderDetail
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
        public decimal? QtyDlv { get; set; }

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

        [Column("CoaCOGS")]
        [StringLength(6)]
        public string CoaCogs { get; set; }

        [StringLength(6)]
        public string CoaSls { get; set; }

        [StringLength(6)]
        public string CoaSlsDisc { get; set; }

        [StringLength(6)]
        public string CoaSlsReturn { get; set; }
    }

    public class VwSalesOrderDetail
    {
        public long Id { get; set; }

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

        public string DimensionMeasurement { get; set; }

        public string WeightMeasurement { get; set; }

        public decimal? QtyDlv { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Disc { get; set; }

        public int? TaxId { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal NettPrice { get; set; }

        public decimal Total { get; set; }

        public decimal Dpp { get; set; }

        public string Notes { get; set; }

        public string CoaInventory { get; set; }

        public string CoaCogs { get; set; }

        public string CoaSls { get; set; }

        public string CoaSlsDisc { get; set; }

        public string CoaSlsReturn { get; set; }


        public string ItemInitial { get; set; }

        public string ItemName { get; set; }

        public int? ItemUomSellId { get; set; }

        public string ItemUomSellName { get; set; }

        public decimal? ItemSellPrice { get; set; }

        public string UomInitial { get; set; }
        
        public string UnitName { get; set; }
    }

    [Table("SalesOrderDetailDiscount", Schema = Schema.Sales)]
    public class SalesOrderDetailDiscount
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

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Value { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaCode { get; set; }
    }

    [Table("SalesOrderDetailFreeGood", Schema = Schema.Sales)]
    public class SalesOrderDetailFreeGood
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

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal QtyClosed { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal UnitPrice { get; set; }

        [StringLength(6)]
        public string CoaCode { get; set; }
    }
}
