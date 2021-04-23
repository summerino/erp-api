using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.SystemManagement
{
    [Table("SequenceNumber", Schema = Schema.SystemManagement)]
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
