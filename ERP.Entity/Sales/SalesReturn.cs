using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Sales
{
    [Table("SalesReturnHeader", Schema = Schema.Sales)]
    public class SalesReturnHeader : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public short SrcTrans { get; set; }

        [StringLength(17)]
        public string TransCode { get; set; }

        public short Type { get; set; }

        [Required]
        [StringLength(8)]
        public string CustCode { get; set; }

        [Required]
        [StringLength(8)]
        public string WarehouseCode { get; set; }

        public long SalesBy { get; set; }

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
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalDisc { get; set; }

        public bool NoTax { get; set; }

        public bool IncludeTax { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; }

        [Column("DPP", TypeName = "decimal(18, 2)")]
        public decimal Dpp { get; set; }

        [StringLength(50)]
        public string TaxInvoiceNo { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TaxInvoiceDate { get; set; }

        [Required]
        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwSalesReturnHeader : BaseEntityWithMarkAndApproved
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public short SrcTrans { get; set; }

        public string TransCode { get; set; }

        public short Type { get; set; }

        public string CustCode { get; set; }

        public string WarehouseCode { get; set; }

        public long SalesBy { get; set; }

        public string CurrCode { get; set; }

        public decimal Rate { get; set; }

        public decimal ShipmentFee { get; set; }

        public decimal HandlingFee { get; set; }

        public decimal SubTotal { get; set; }

        public decimal FinalDisc { get; set; }

        public bool NoTax { get; set; }

        public bool IncludeTax { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal Total { get; set; }

        public decimal Dpp { get; set; }

        public string TaxInvoiceNo { get; set; }

        public DateTime? TaxInvoiceDate { get; set; }

        public string Notes { get; set; }
         

        public string CustName { get; set; }

        public string SalesInitial { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string TypeName { get; set; }

        public string Status { get; set; }
    }

    [Table("SalesReturnDetail", Schema = Schema.Sales)]
    public class SalesReturnDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public long? TransDetailId { get; set; }

        public int ItemId { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal QtyDlv { get; set; }

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
    }

    public class VwSalesReturnDetail
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public short LineNo { get; set; }

        public long? TransDetailId { get; set; }

        public int ItemId { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        public decimal Qty { get; set; }

        public decimal QtyDlv { get; set; }

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

        public string ItemInitial { get; set; }

        public string ItemName { get; set; }

        public int? ItemUomSellId { get; set; }

        public string ItemUomSellName { get; set; }

        public decimal? ItemSellPrice { get; set; }

        public string UomInitial { get; set; }

        public string UnitName { get; set; }
    }

    [Table("SalesReturnDetailExchDiffItem", Schema = Schema.Sales)]
    public class SalesReturnDetailExchDiffItem
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public long? ReturnDetailId { get; set; }

        public int ItemId { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Qty { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal QtyDlv { get; set; }

        [StringLength(8)]
        public string WarehouseCode { get; set; }
        
        [Column(TypeName = "decimal(19, 6)")]
        public decimal UnitPrice { get; set; }

        public int? TaxId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal NettPrice { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Total { get; set; }

        [Column("DPP", TypeName = "decimal(19, 6)")]
        public decimal Dpp { get; set; }
    }

    public class VwSalesReturnDetailExchDiffItem 
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public short LineNo { get; set; }

        public long? ReturnDetailId { get; set; }

        public int ItemId { get; set; }
        
        public int UomId { get; set; }

        public int UnitId { get; set; }

        public decimal Qty { get; set; }

        public decimal QtyDlv { get; set; }

        public string WarehouseCode { get; set; }

        public decimal UnitPrice { get; set; }

        public int? TaxId { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal NettPrice { get; set; }

        public decimal Total { get; set; }

        public decimal Dpp { get; set; }


        public string ItemInitial { get; set; }

        public string ItemName { get; set; }

        public int? ItemUomSellId { get; set; }

        public string ItemUomSellName { get; set; }

        public decimal? ItemSellPrice { get; set; }

        public string UomInitial { get; set; }

        public string UnitName { get; set; }
    }
}
