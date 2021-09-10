using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Accounting
{
    [Table("IncomeStatementFormat", Schema = Schema.Accounting)]
    public class IncomeStatementFormat : BaseEntityWithActive
    {
        [Key]
        [StringLength(5)]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(5)]
        public string ParentCode { get; set; }

        [Required]
        [StringLength(1)]
        public string Category { get; set; }

        [Required]
        [StringLength(1)]
        public string Type { get; set; }

        [Required]
        [StringLength(5)]
        public string PercentOf { get; set; }

        [StringLength(5)]
        public string PercentFrom { get; set; }

        [Required]
        [StringLength(1)]
        public string Position { get; set; }

        public int Deep { get; set; }

        public int Sort { get; set; }

        public int SubtotalSort { get; set; }

        public bool Detail { get; set; }

        public bool Hidden { get; set; }

        public bool Bold { get; set; }

        public bool ByAccount { get; set; }
    }

    [Table("IncomeStatementFormatSubtotal", Schema = Schema.Accounting)]
    public class IncomeStatementFormatSubtotal
    {
        public long Id { get; set; }

        [Required]
        [StringLength(5)]
        public string Code { get; set; }

        [Required]
        [StringLength(5)]
        public string SubCode { get; set; }

        public int CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; }
    }

    public class VwIncomeStatementFormatSubtotal
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string SubCode { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }


        public string SubName { get; set; }

        public string SubPosition { get; set; }
    }
}
