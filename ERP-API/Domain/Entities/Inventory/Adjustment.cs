using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Inventory
{
    [Table("AdjustmentHeader", Schema = Schema.Inventory)]
    public class AdjustmentHeader : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string  Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        
        public short Type { get; set; }

        [Required]
        [StringLength(8)]
        public string WarehouseCode { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwAdjustmentHeader : BaseEntityWithMarkAndApproved
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public short Type { get; set; }
        public string WarehouseCode { get; set; }
        public string Notes { get; set; }
        public string WarehouseInitial { get; set; }
        public string CreatedInitial { get; set; }
        public string UpdatedInitial { get; set; }
        public string ApprovedInitial { get; set; }
        public string Status { get; set; }
    }
    
    [Table("AdjustmentDetail", Schema = Schema.Inventory)]
    public class AdjustmentDetail
    {
        public AdjustmentDetail()
        {
            DifferentUnits = new HashSet<AdjustmentDetailDiffUnit>();
        }
        [Key]
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal QtyOnHand { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal QtyOnTransfer { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal QtyAdjust { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseQtyOnHand { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseQtyOnTransfer { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal COGS { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
        public IEnumerable<AdjustmentDetailDiffUnit> DifferentUnits { get; set; }
    }

    [Table("AdjustmentDetailDiffUnit", Schema = Schema.Inventory)]
    public class AdjustmentDetailDiffUnit
    {
        [Key]
        public long Id { get; set; }

        public long AdjustmentDetailId { get; set; }

        public int UnitId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal QtyAdjust { get; set; }
    }

    public class VwAdjustmentDetail
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        public decimal QtyOnHand { get; set; }

        public decimal QtyOnTransfer { get; set; }

        public decimal QtyAdjust { get; set; }

        public decimal BaseQtyOnHand { get; set; }

        public decimal BaseQtyOnTransfer { get; set; }

        public decimal COGS { get; set; }

        public string Notes { get; set; }


        public decimal QtyOpname { get; set; }

        public decimal Different { get; set; }

        public string ItemName { get; set; }
    }

    public class VwAdjustmentItem : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        public int CategoryId { get; set; }

        public short TypeId { get; set; }

        public decimal? CostOfGoodSold { get; set; }

        public short? ValuationMethod { get; set; }

        public short? StockType { get; set; }

        public int? UomId { get; set; }

        public int? UomSellId { get; set; }

        public decimal? SellPrice { get; set; }

        public int? UomBuyId { get; set; }

        public decimal? BuyPrice { get; set; }

        public int? SalesTaxId { get; set; }

        public int? PurchaseTaxId { get; set; }

        [StringLength(50)]
        public string Category1 { get; set; }

        [StringLength(50)]
        public string Category2 { get; set; }

        [StringLength(50)]
        public string Category3 { get; set; }

        [StringLength(50)]
        public string Category4 { get; set; }

        [StringLength(50)]
        public string Category5 { get; set; }

        [StringLength(50)]
        public string SubGroup1 { get; set; }

        [StringLength(50)]
        public string SubGroup2 { get; set; }

        [StringLength(50)]
        public string SubGroup3 { get; set; }

        [StringLength(50)]
        public string SubGroup4 { get; set; }

        [StringLength(50)]
        public string SubGroup5 { get; set; }

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

        [StringLength(6)]
        public string CoaSls { get; set; }

        [StringLength(6)]
        public string CoaSlsReturn { get; set; }

        [StringLength(6)]
        public string CoaSlsDisc { get; set; }

        [StringLength(6)]
        public string CoaOffSet { get; set; }

        [StringLength(6)]
        public string CoaCost { get; set; }

        [StringLength(6)]
        public string CoaExpense { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Length { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Width { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Height { get; set; }

        [StringLength(10)]
        public string DimensionMeasurement { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal? Weight { get; set; }

        [StringLength(10)]
        public string WeightMeasurement { get; set; }
        public string TypeName { get; set; }
        public string CategoryName { get; set; }
        public string UomInitial { get; set; }

        public string UomSellName { get; set; }

        public string UomBuyName { get; set; }
        public decimal QtyOnHand { get; set; }
        public string WarehouseCode { get; set; }
    }

}
