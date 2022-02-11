using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Accounting
{
    [Table("GeneralJournalHeader", Schema = Schema.Accounting)]
    public class GeneralJournalHeader : BaseEntityWithMarkApprovedAndViewed
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Precision(19, 6)]
        public decimal Rate { get; set; }

        [Precision(18, 2)]
        public decimal Total { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwGeneralJournalHeader : BaseEntityWithMarkApprovedAndViewed
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public string CurrCode { get; set; }

        [Precision(19, 6)]
        public decimal Rate { get; set; }

        [Precision(18, 2)]
        public decimal Total { get; set; }

        public string Notes { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }
    }

    [Table("GeneralJournalDetail", Schema = Schema.Accounting)]
    [Index(nameof(CoaCode))]
    public class GeneralJournalDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaCode { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }

        [Required]
        [StringLength(1)]
        public string Type { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }
    }

    [Table("Journal", Schema = Schema.Accounting)]
    [Index(nameof(Code))]
    [Index(nameof(CoaCode))]
    [Index(nameof(TypeCode))]
    [Index(nameof(RefCode1))]
    [Index(nameof(RefCode2))]
    [Index(nameof(RefCode3))]
    [Index(nameof(RefCode4))]
    [Index(nameof(Period))]
    [Index(nameof(SrcTrans))]
    public class Journal
    {
        public long Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaCode { get; set; }

        [Required]
        [StringLength(20)]
        public string TypeCode { get; set; }

        [Required]
        [StringLength(256)]
        public string Notes { get; set; }

        [StringLength(20)]
        public string RefCode1 { get; set; }

        [StringLength(20)]
        public string RefCode2 { get; set; }

        [StringLength(20)]
        public string RefCode3 { get; set; }

        [StringLength(20)]
        public string RefCode4 { get; set; }

        public short Group { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Required]
        [StringLength(8)]
        public string Period { get; set; }

        [Precision(19, 6)]
        public decimal CustomRate { get; set; }

        [Required]
        [StringLength(1)]
        public string Type { get; set; }

        [Precision(19, 6)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string SrcTrans { get; set; }
    }
}
