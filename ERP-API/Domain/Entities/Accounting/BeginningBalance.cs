using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Accounting
{
    [Table("BeginningBalanceAP", Schema = Schema.Accounting)]
    [Index(nameof(Code), IsUnique = true)]
    public class BeginningBalanceAP : BaseEntityWithActive
    {
        public long Id { get; set; }

        [Required]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal CurrRate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwBeginningBalanceAP : BaseEntityWithActive
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public DateTime Date { get; set; }

        public DateTime DueDate { get; set; }

        public string SupCode { get; set; }

        public string CurrCode { get; set; }

        public decimal CurrRate { get; set; }

        public decimal Amount { get; set; }

        public string Notes { get; set; }

        public string SupName { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }
    }

    [Table("BeginningBalanceAR", Schema = Schema.Accounting)]
    [Index(nameof(Code), IsUnique = true)]
    public class BeginningBalanceAR : BaseEntityWithActive
    {
        public long Id { get; set; }

        [Required]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(8)]
        public string CustCode { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal CurrRate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwBeginningBalanceAR : BaseEntityWithActive
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public DateTime Date { get; set; }

        public DateTime DueDate { get; set; }

        public string CustCode { get; set; }

        public string CurrCode { get; set; }

        public decimal CurrRate { get; set; }

        public decimal Amount { get; set; }

        public string Notes { get; set; }

        public string CustName { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }
    }
}
