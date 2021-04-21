using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.SystemManagement
{
    [Table("Menu", Schema = Schema.SystemManagement)]
    public class Menu : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int? ParentId { get; set; }

        public int Deep { get; set; }

        public int Seq { get; set; }

        [StringLength(100)]
        public string Link { get; set; }

        [StringLength(50)]
        public string Icon { get; set; }
    }
}
