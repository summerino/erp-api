using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Catalog
{
    [Table("Tenant")]
    public class Tenant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(128)]
        public string ServerName { get; set; }

        [Required]
        [StringLength(128)]
        public string DatabaseName { get; set; }

        [Required]
        [Column(TypeName = "nvarchar")]
        [StringLength(20)]
        public string ServerUserId { get; set; }

        [Required]
        [StringLength(128)]
        public string ServerPassword { get; set; }
    }
}
