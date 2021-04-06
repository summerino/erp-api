using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_API.Entities
{
    [Table("msSystemParameters", Schema = "dbo")]
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
