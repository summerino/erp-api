using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;
using Microsoft.EntityFrameworkCore;

namespace ERP.Entity.Accounting
{
    [Table("CurrencyRate", Schema = Schema.Accounting)]
    [Index(nameof(Date), nameof(CurrCode), IsUnique = true)]
    public class CurrencyRate : BaseEntity
    {
        public long Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Amount { get; set; }
    }
}
