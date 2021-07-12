using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.General
{
    [Table("Currency", Schema = Schema.General)]
    public class Currency : BaseEntityWithActive
    {
        [Key]
        [StringLength(3)]
        public string Code { get; set; }

        [Required]
        [StringLength(32)]
        [Column(TypeName = "nvarchar")]
        public string Name { get; set; }

        public int Sort { get; set; }
    }
}
