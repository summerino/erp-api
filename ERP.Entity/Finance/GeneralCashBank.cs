using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Finance
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

        public string TypeName { get; set; }

        public string Status { get; set; }
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

        [StringLength(5)]
        public string Src { get; set; }
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

        public string Src { get; set; }


        public string CoaName { get; set; }
    }

    public class VwInterCashBankHeader : BaseEntityWithMarkAndApproved
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


        public string TransCode { get; set; }

        public string CoaCodeTo { get; set; }

        public string CurrDetail { get; set; }

        public decimal RateDetail { get; set; }

        public decimal AmountDetail { get; set; }

        public string TypeDetail { get; set; }

        public string TypeAmount { get; set; }

        public decimal TransAmount { get; set; }

        public string NotesDetail { get; set; }

        public string CoaNameFrom { get; set; }

        public string CoaNameTo { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string Status { get; set; }
    }

    [Table("CashBankType", Schema = Schema.Finance)]
    [Index(nameof(SysParCode))]
    public class CashBankType
    {
        [Key]   
        [StringLength(5)]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string SysParCode { get; set; }

        public short Seq { get; set; }

        public bool IsActive { get; set; }
    }

    public class VwCashBankType
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string SysParCode { get; set; }

        public short Seq { get; set; }

        public bool IsActive { get; set; }


        public string CoaCode { get; set; }

        public string CoaName { get; set; }

        public int ActionId { get; set; }
    }

    public class VwAR
    {
        public string Code { get; set; }

        public string CustCode { get; set; }

        public string CustName { get; set; }
        
        public string CurrCode { get; set; }

        public decimal Rate { get; set; }

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal Remaining { get; set; }

        public string Notes { get; set; }

        public string Src { get; set; }
    }

    public class VwAP
    {
        public string Code { get; set; }

        public string SupCode { get; set; }

        public string SupName { get; set; }

        public string CurrCode { get; set; }

        public decimal Rate { get; set; }

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal Remaining { get; set; }

        public string Notes { get; set; }

        public string Src { get; set; }
    }

    public class VwOutstandingCreditMemo
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public string CustCode { get; set; }

        public string CustName { get; set; }

        public short Type { get; set; }

        public string CurrCode { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

        public decimal Used { get; set; }

        public decimal Remaining { get; set; }

        public string Notes { get; set; }

        public string Src { get; set; }
    }

    public class VwOutstandingDebitMemo
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public string SupCode { get; set; }

        public string SupName { get; set; }

        public short Type { get; set; }

        public string CurrCode { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

        public decimal Used { get; set; }

        public decimal Remaining { get; set; }

        public string Notes { get; set; }

        public string Src { get; set; }
    }

    public class VwDebitCreditPayment
    {
        public string TransCode { get; set; }
        public decimal Amount { get; set; }
        public string TypeAmount { get; set; }
    }

}
