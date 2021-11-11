using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales
{
    [Table("MobileActivityLog", Schema = Schema.MobileSales)]
    [Index(nameof(Date), nameof(Username), nameof(TypeCode))]
    public class MobileActivityLog
    {
        public long Id { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(10)]
        public string TypeCode { get; set; }

        [Required]
        [StringLength(256)]
        public string Notes { get; set; }
    }
}
