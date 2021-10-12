using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileWarehouse
{
    [Table("MobileReceiveItemHeader", Schema = Schema.MobileWarehouse)]
    [Index(nameof(TransCode))]
    public class MobileReceiveItemHeader : BaseEntityWithMarkApprovedAndRejected
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [StringLength(17)]
        public string RcvCode { get; set; }

        [Required]
        [StringLength(17)]
        public string TransCode { get; set; }

        public short SrcTrans { get; set; }

        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }

        public long ReceiveBy { get; set; }
    }

    [Table("MobileReceiveItemDetail", Schema = Schema.MobileWarehouse)]
    public class MobileReceiveItemDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public long? TransDetailId { get; set; }

        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Qty { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        [Required]
        [StringLength(8)]
        public string WarehouseCode { get; set; }

        public int Type { get; set; }
    }
}
