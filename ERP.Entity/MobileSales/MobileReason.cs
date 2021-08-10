using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales
{
    [Table("MobileReason", Schema = Schema.MobileSales)]
    [Index(nameof(Type))]
    public class MobileReason : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(5)]
        public string Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
