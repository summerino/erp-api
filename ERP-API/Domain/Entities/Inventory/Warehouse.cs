using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Inventory
{
    [Table("msWarehouses", Schema = "dbo")]
    public class Warehouse : BaseEntityWithActive
    {
        [Key]
        [StringLength(8)]
        public string Code { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        public bool IsDefault { get; set; }
    }
}
