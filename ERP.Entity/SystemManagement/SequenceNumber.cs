using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.SystemManagement
{
    [Table("SequenceNumber", Schema = Schema.SystemManagement)]
    [Index(nameof(Code))]
    [Index(nameof(Format))]
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
