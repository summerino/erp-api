using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Finance
{
    [Table("GeneralCashBankHeader", Schema = Schema.Finance)]
    [Index(nameof(CoaCode))]
    public class GeneralCashBankHeader : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [StringLength(17)]
        public string VouCode { get; set; }

        [Required]
        [StringLength(1)]
        public string Type { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaCode { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(25)]
        public string ChequeNo { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ChequeDate { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }

        public bool IsInterCashBank { get; set; }
    }

    public class VwGeneralCashBankHeader : BaseEntityWithMarkAndApproved
    {
        public string Code { get; set; }

        public string VouCode { get; set; }

        public string Type { get; set; }

        public DateTime Date { get; set; }

        public string CoaCode { get; set; }

        public string CurrCode { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

        public string ChequeNo { get; set; }

        public DateTime? ChequeDate { get; set; }

        public string Notes { get; set; }

        public bool IsInterCashBank { get; set; }


        public string CoaName{ get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }
    }

    [Table("GeneralCashBankDetail", Schema = Schema.Finance)]
    [Index(nameof(TransCode))]
    [Index(nameof(CoaCode))]
    public class GeneralCashBankDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        [Required]
        [StringLength(5)]
        public string Type { get; set; }

        [StringLength(17)]
        public string TransCode { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaCode { get; set; }


        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(1)]
        public string TypeAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TransAmount { get; set; }

        [Required]
        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwGeneralCashBankDetail
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public short LineNo { get; set; }

        public string Type { get; set; }

        public string TransCode { get; set; }

        public string CoaCode { get; set; }

        public string CurrCode { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

        public string TypeAmount { get; set; }

        public decimal TransAmount { get; set; }

        public string Notes { get; set; }
        

        public string CoaName { get; set; }
    }

    [Table("CashBankType", Schema = Schema.Finance)]
    public class CashBankType
    {
        [Key]
        [StringLength(5)]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public short Seq { get; set; }

        public bool IsActive { get; set; }
    }
}
