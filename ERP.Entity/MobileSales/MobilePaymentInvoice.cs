using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales
{
    [Table("MobilePaymentInvoice", Schema = Schema.MobileSales)]
    [Index(nameof(CustCode))]
    [Index(nameof(CoaCode))]
    [Index(nameof(TransCode))]
    public class MobilePaymentInvoice : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Required]
        [StringLength(17)]
        public string VisitLogCode { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public long SalesmanId { get; set; }

        [Required]
        [StringLength(8)]
        public string CustCode { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaCode { get; set; }

        [Required]
        [StringLength(17)]
        public string TransCode { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(256)]
        public string NotesFailCollect { get; set; }

        [Required]
        [StringLength(5)]
        public string SrcTrans { get; set; }
    }
}
