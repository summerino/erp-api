using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Inventory
{
    [Table("AdjustmentHeader", Schema = Schema.Inventory)]
    public class AdjustmentHeader : BaseEntityWithMark
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
    public class VwAdjustmentHeader : BaseEntityWithMark
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public short Type { get; set; }
        public string Warehouse { get; set; }
        public string WarehouseCode { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
    }

    
    [Table("AdjustmentDetail", Schema = Schema.Inventory)]
    public class AdjustmentDetail
    {
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
        //[Column(TypeName = "decimal(19, 6)")]
        //public decimal QtyOnTransfer { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal QtyAdjust { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseQtyOnHand { get; set; }
        //[Column(TypeName = "decimal(18, 2)")]
        //public decimal BaseQtyOnTransfer { get; set; }
        [Column(TypeName = "decimal(19, 6)")]
        public decimal COGS { get; set; }
        [StringLength(256)]
        public string Notes { get; set; }


    }

    public class VwAdjustmentDetail
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public short LineNo { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int UomId { get; set; }
        public int UnitId { get; set; }
        public decimal QtyOnHand { get; set; }
        public decimal QtyAdjust { get; set; }
        public decimal QtyOpname { get; set; }
        public decimal Different { get; set; }
        public decimal BaseQtyOnHand { get; set; }
        public decimal COGS { get; set; }
        public string Notes { get; set; }
    }
}
