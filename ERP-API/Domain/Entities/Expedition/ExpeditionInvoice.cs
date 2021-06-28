using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Expedition
{
    [Table("ExpeditionInvoiceHeader", Schema = Schema.Expedition)]
    public class ExpeditionInvoiceHeader : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        public short SrcTrans { get; set; }

        [StringLength(30)]
        public string RefNo { get; set; }

        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwExpeditionInvoiceHeader : BaseEntityWithMarkAndApproved
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public DateTime DueDate { get; set; }

        public short SrcTrans { get; set; }

        public string RefNo { get; set; }

        public string SupCode { get; set; }

        public string CurrCode { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

        public string Notes { get; set; }


        public string SupplierInitial { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }
    }

    [Table("ExpeditionInvoiceDetail", Schema = Schema.Expedition)]
    public class ExpeditionInvoiceDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        [Required]
        [StringLength(17)]
        public string TransCode { get; set; }
    }
}
