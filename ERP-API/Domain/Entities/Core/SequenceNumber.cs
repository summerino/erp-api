using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP_API.Domain.Entities.Core
{
    [Table("msSequenceNumbers", Schema = "dbo")]
    public class SequenceNumber
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(20)]
        public string Format { get; set; }

        public int LastRunNo { get; set; }
    }
}
