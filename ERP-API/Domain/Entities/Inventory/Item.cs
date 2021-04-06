using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Entities;

namespace ERP_API.Domain.Entities.Inventory
{
    [Table("msItems", Schema = "dbo")]
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
    }

    [Table("vw_items", Schema = "dbo")]
    public class VwItem : BaseEntityWithActive
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


        public string TypeName { get; set; }

        public string CategoryName { get; set; }

        public string UomInitial { get; set; }
        
        public string UomSellName { get; set; }

        public string UomBuyName { get; set; }
    }
}
