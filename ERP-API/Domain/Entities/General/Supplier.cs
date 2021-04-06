using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Entities;

namespace ERP_API.Domain.Entities.General
{
    public class Supplier : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("SupplierID")]
        public int SupplierId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Address {get; set;}
        public string Description { get; set; }
    }
}
