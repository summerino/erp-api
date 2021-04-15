using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Purchase
{
    [Table("PurchaseInvoiceHeader", Schema = Schema.Purchasing)]
    public class PurchaseInvoiceHeader : BaseEntityWithMark
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        [Column("POCode")]
        [StringLength(17)]
        public string PoCode { get; set; }

        [StringLength(30)]
        public string RefNo { get; set; }

        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }

        public long IssuedBy { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal InvoiceAmount { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwPurchaseInvoiceHeader : BaseEntityWithMark
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public DateTime DueDate { get; set; }

        public string PoCode { get; set; }

        public string RefNo { get; set; }

        public string SupCode { get; set; }

        public long IssuedBy { get; set; }

        public string CurrCode { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal InvoiceAmount { get; set; }

        public string Notes { get; set; }


        public string SupName { get; set; }

        public string IssuedInitial { get; set; }
    }

    [Table("PurchaseInvoiceDetail", Schema = Schema.Purchasing)]
    public class PurchaseInvoiceDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        [Column("PODetailId")]
        public long? PoDetailId { get; set; }

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
}
