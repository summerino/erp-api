using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.MobileWarehouse
{
    [Table("MobileDeliveryItemHeader", Schema = Schema.MobileWarehouse)]
    public class MobileDeliveryItemHeader : BaseEntityWithMarkApprovedAndRejected
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Required]
        [StringLength(17)]
        public string DlvPlanCode { get; set; }
    }

    [Table("MobileDeliveryItemDetail", Schema = Schema.MobileWarehouse)]
    public class MobileDeliveryItemDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Qty { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }
    }
}
