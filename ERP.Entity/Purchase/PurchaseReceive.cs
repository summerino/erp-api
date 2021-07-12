using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Purchase
{
    [Table("PurchaseReceiveHeader", Schema = Schema.Purchasing)]
    [Index(nameof(TransCode))]
    public class PurchaseReceiveHeader : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(17)]
        public string TransCode { get; set; }

        [StringLength(30)]
        public string RefNo { get; set; }

        public short SrcTrans { get; set; }

        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }

        public long ReceiveBy { get; set; }

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

        [Column(TypeName = "decimal(19, 6)")]
        public decimal PaidAmount { get; set; }
    }

    public class VwPurchaseReceiveHeader : BaseEntityWithMarkAndApproved
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public string TransCode { get; set; }

        public string RefNo { get; set; }

        public short SrcTrans { get; set; }

        public string SupCode { get; set; }

        public long ReceiveBy { get; set; }

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
        
        public decimal PaidAmount { get; set; }


        public string SupName { get; set; }

        public string ReceiveInitial { get; set; }

        public string CreatedInitial { get; set; }
        
        public string UpdatedInitial { get; set; }

        public string Status { get; set; }
    }

    [Table("PurchaseReceiveDetail", Schema = Schema.Purchasing)]
    public class PurchaseReceiveDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public long? TransDetailId { get; set; }

        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Qty { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

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

        [Required]
        [StringLength(8)]
        public string WarehouseCode { get; set; }

        public int Type { get; set; }
    }

    public class VwPurchaseReceiveDetail
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public short LineNo { get; set; }

        public long? TransDetailId { get; set; }

        public int ItemId { get; set; }

        public decimal Qty { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        public decimal? Weight { get; set; }

        public string DimensionMeasurement { get; set; }

        public string WeightMeasurement { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Disc { get; set; }

        public int? TaxId { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal NettPrice { get; set; }

        public decimal Total { get; set; }

        public decimal Dpp { get; set; }

        public string WarehouseCode { get; set; }

        public int Type { get; set; }


        public decimal OrderQty { get; set; }

        public decimal OutstandingQty { get; set; }

        public string ItemInitial { get; set; }

        public string ItemName { get; set; }

        public int? ItemUomBuyId { get; set; }

        public string ItemUomBuyName { get; set; }

        public decimal? ItemBuyPrice { get; set; }

        public string UomInitial { get; set; }

        public string UnitName { get; set; }
    }
}
