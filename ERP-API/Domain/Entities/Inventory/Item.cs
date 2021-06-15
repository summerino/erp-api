using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Inventory
{
    [Table("Item", Schema = Schema.Inventory)]
    public class Item : BaseEntityWithActive
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

        [Column(TypeName = "decimal(19, 6)")]
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
        [Column("CoaCOGS")]
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
    }

    public class VwItem : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

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

        public string Category1 { get; set; }

        public string Category2 { get; set; }

        public string Category3 { get; set; }

        public string Category4 { get; set; }

        public string Category5 { get; set; }

        public string SubGroup1 { get; set; }

        public string SubGroup2 { get; set; }

        public string SubGroup3 { get; set; }

        public string SubGroup4 { get; set; }

        public string SubGroup5 { get; set; }

        public string CoaInventory { get; set; }

        public string CoaCogs { get; set; }

        public string CoaPurc { get; set; }

        public string CoaPurcDisc { get; set; }

        public string CoaPurcReturn { get; set; }

        public string CoaSls { get; set; }

        public string CoaSlsReturn { get; set; }

        public string CoaSlsDisc { get; set; }

        public string CoaOffSet { get; set; }

        public string CoaCost { get; set; }

        public string CoaExpense { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        public string DimensionMeasurement { get; set; }

        public decimal? Weight { get; set; }

        public string WeightMeasurement { get; set; }


        public string TypeName { get; set; }

        public string CategoryName { get; set; }

        public string UomInitial { get; set; }
        
        public string UomSellName { get; set; }

        public string UomBuyName { get; set; }
        public int? BuySeq { get; set; }
        public int? SellSeq { get; set; }

        public decimal? QtyOnHand { get; set; }

        public decimal? QtyOnOrder { get; set; } 

        public decimal? QtyOnIndent { get; set; }

        public decimal? QtyOnTransfer { get; set; }

        public decimal SellQtyAvailable { get; set; }

        public decimal BuyQtyAvailable { get; set; }
        public string WarehouseCode { get; set; }

        public string CreatedInitial { get; set; }
        
        public string UpdatedInitial { get; set; }
    }
}
