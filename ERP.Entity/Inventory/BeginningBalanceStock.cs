using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace ERP.Entity.Inventory
{
    [Table("BeginningBalanceHeader", Schema = Schema.Inventory)]
    public class BeginningBalanceStockHeader : BaseEntityWithActive
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(8)]
        public string WarehouseCode { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwBeginningBalanceStockHeader : BaseEntityWithActive
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public string WarehouseCode { get; set; }

        public string Notes { get; set; }


        public string WarehouseInitial { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }
    }

    [Table("BeginningBalanceDetail", Schema = Schema.Inventory)]
    public class BeginningBalanceStockDetail
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
        public decimal UnitPrice { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }
    public class VwBeginningBalanceStockDetail
    {
        public long Id { get; set; }
        
        public string Code { get; set; }
        
        public short LineNo { get; set; }
        
        public int ItemId { get; set; }
        
        public int UomId { get; set; }
        
        public int UnitId { get; set; }
        
        [Precision(18, 2)]
        public decimal Qty { get; set; }
        
        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }
        
        public string Notes { get; set; }


        public string ItemName { get; set; }
        
        public string ItemInitial { get; set; }
        
        public int? ItemUomBuyId { get; set; }
        
        public string ItemUomBuyName { get; set; }
        
        [Precision(18, 2)]
        public decimal? ItemBuyPrice { get; set; }
    }

    public class VwBeginningBalanceItem : BaseEntityWithActive
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

        [Precision(19, 6)]
        public decimal? CostOfGoodSold { get; set; }

        public short? ValuationMethod { get; set; }

        public short? StockType { get; set; }

        public int? UomId { get; set; }

        public int? UomSellId { get; set; }

        [Precision(18, 2)]
        public decimal? SellPrice { get; set; }

        public int? UomBuyId { get; set; }

        [Precision(18, 2)]
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

        [Precision(18, 2)]
        public decimal? Length { get; set; }

        [Precision(18, 2)]
        public decimal? Width { get; set; }

        [Precision(18, 2)]
        public decimal? Height { get; set; }

        [StringLength(10)]
        public string DimensionMeasurement { get; set; }

        [Precision(18, 3)]
        public decimal? Weight { get; set; }

        [StringLength(10)]
        public string WeightMeasurement { get; set; }

        public string TypeName { get; set; }

        public string CategoryName { get; set; }

        public string UomInitial { get; set; }

        public string UomSellName { get; set; }

        public string UomBuyName { get; set; }

        [Precision(19, 6)]
        public decimal QtyOnHand { get; set; }

        public string WarehouseCode { get; set; }
    }
}
