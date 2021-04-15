using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.General
{
    [Table("msCurrencies", Schema = "dbo")]
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
