using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Inventory
{
    [Table("TransferStockHeader", Schema = Schema.Inventory)]
    public class TransferStockHeader : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string  Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public short Type { get; set; }

        [StringLength(17)]
        public string OriginTransferCode { get; set; }

        [StringLength(8)]
        public string WarehouseCodeFrom { get; set; }

        [StringLength(8)]
        public string WarehouseCodeTo { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwTransferStockHeader : BaseEntityWithMarkAndApproved
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public short Type { get; set; }

        public string OriginTransferCode { get; set; }

        public string WarehouseCodeFrom { get; set; }

        public string WarehouseCodeTo { get; set; }

        public string Notes { get; set; }


        public string WarehouseInitialFrom { get; set; }

        public string WarehouseInitialTo { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string Status { get; set; }
    }
    
    [Table("TransferStockDetail", Schema = Schema.Inventory)]
    public class TransferStockDetail
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
        public decimal Qty { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwTransferStockDetail
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        public decimal Qty { get; set; }

        public string Notes { get; set; }


        public string ItemName { get; set; }
    }
}
