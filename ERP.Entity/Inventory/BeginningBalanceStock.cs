using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

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

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Qty { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }
}
