using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales
{
    [Table("MobilePaymentMethod", Schema = Schema.MobileSales)]
    public class MobilePaymentMethod : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaCode { get; set; }

        public short Seq { get; set; }
    }
}
