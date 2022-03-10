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
        
        [Precision(19, 6)]
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
        
        [Precision(19, 6)]
        public decimal UnitPrice { get; set; }
        
        public string Notes { get; set; }


        public string ItemName { get; set; }
        
        public string ItemInitial { get; set; }
        
        public int? ItemUomBuyId { get; set; }
        
        public string ItemUomBuyName { get; set; }
        
        [Precision(18, 2)]
        public decimal? ItemBuyPrice { get; set; }
    }
}
