using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Inventory
{
    [Table("WarehouseQuantity", Schema = Schema.Inventory)]
    public class WarehouseQuantity
    {
        public long Id { get; set; }

        [Required]
        [StringLength(8)]
        public string WarehouseCode { get; set; }

        public int ItemId { get; set; }

        [Precision(19, 6)]
        public decimal QtyOnHand { get; set; }

        [Precision(19, 6)]
        public decimal QtyOnOrder { get; set; }

        [Precision(19, 6)]
        public decimal QtyOnIndent { get; set; }

        [Precision(19, 6)]
        public decimal QtyReorderPoint { get; set; }

        [Precision(19, 6)]
        public decimal QtyOnTransfer { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedDate { get; set; }
    }

    public class VwWarehouseQuantity
    {
        public long Id { get; set; }

        public string WarehouseCode { get; set; }

        public int ItemId { get; set; }

        public decimal QtyOnHand { get; set; }

        public decimal QtyOnOrder { get; set; }

        public decimal QtyOnIndent { get; set; }

        public decimal QtyReorderPoint { get; set; }

        public decimal QtyOnTransfer { get; set; }

        public DateTime UpdatedDate { get; set; }


        public string WarehouseInitial { get; set; }
    }
}
