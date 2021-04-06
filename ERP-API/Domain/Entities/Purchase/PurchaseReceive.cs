using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Entities;

namespace ERP_API.Domain.Entities.Purchase
{
    [Table("trPurchaseReceiveHeaders", Schema = "dbo")]
    public class PurchaseReceiveHeader : BaseEntityWithMark
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [Column("POCode")]
        [StringLength(17)]
        public string PoCode { get; set; }

        [StringLength(30)]
        public string RefNo { get; set; }

        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }

        [Required]
        public long ReceiveBy { get; set; }

        public long? ApproveBy { get; set; }

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

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; }

        [Column("DPP", TypeName = "decimal(18, 2)")]
        public decimal Dpp { get; set; }
    }

    [Table("trPurchaseReceiveDetails", Schema = "dbo")]
    public class PurchaseReceiveDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OrderQty { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal OutstandingQty { get; set; }

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

        public bool IncludeTax { get; set; }

        public int? TaxId { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
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

        [StringLength(256)]
        public string Notes { get; set; }

        public int Type { get; set; }
    }
}
