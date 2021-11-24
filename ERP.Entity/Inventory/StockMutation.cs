using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Inventory
{
    [Table("StockMutation", Schema = Schema.Inventory)]
    [Index(nameof(RefCode1))]
    [Index(nameof(RefDetailId1))]
    [Index(nameof(RefCode2))]
    [Index(nameof(Type))]
    [Index(nameof(Src))]
    public class StockMutation
    {
        public long Id { get; set; }

        [Required]
        [StringLength(8)]
        public string WarehouseCode { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public int ItemId { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        [Precision(18, 6)]
        public decimal Qty { get; set; }

        [Precision(19, 6)]
        public decimal NettPrice { get; set; }

        public int BaseUnit { get; set; }

        [Precision(18, 6)]
        public decimal BaseQty { get; set; }

        [Precision(19, 6)]
        public decimal BaseNettPrice { get; set; }

        [Required]
        [StringLength(17)]
        public string RefCode1 { get; set; }

        public long RefDetailId1 { get; set; }

        [StringLength(17)]
        public string RefCode2 { get; set; }

        [Required]
        [StringLength(5)]
        public string Type { get; set; }

        [Required]
        [StringLength(10)]
        public string Src { get; set; }
    }
}
