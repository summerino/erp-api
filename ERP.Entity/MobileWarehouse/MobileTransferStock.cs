using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.MobileWarehouse
{
    [Table("MobileTransferStockHeader", Schema = Schema.MobileWarehouse)]
    public class MobileTransferStockHeader : BaseEntityWithMarkApprovedAndRejected
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Required]
        [StringLength(17)]
        public string TransferCode { get; set; }
    }

    [Table("MobileTransferStockDetail", Schema = Schema.MobileWarehouse)]
    public class MobileTransferStockDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal OriginalQty { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal RealizeQty { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }
    }
}
