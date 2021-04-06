using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_API.Entities.Control
{
    public class TenantDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("TenantShard")]
        public Guid Shard { get; set; }

        public string TenantName { get; set; }

        public string TenantSlug { get; set; }

        [Required]
        [StringLength(128)]
        public string ServerName { get; set; }

        [Required]
        [StringLength(128)]
        public string DatabaseName { get; set; }

        [Required]
        [StringLength(128)]
        public string ServerUserId { get; set; }

        [Required]
        [StringLength(128)]
        public string ServerPassword { get; set; }
    }
}
