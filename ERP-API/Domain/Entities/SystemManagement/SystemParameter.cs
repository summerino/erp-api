using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.SystemManagement
{
    [Table("SystemParameter", Schema = Schema.SystemManagement)]
    public class SystemParameter
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string Module { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        public string Value { get; set; }

        public int? Seq { get; set; }
    }
}
