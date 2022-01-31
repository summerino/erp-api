using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Purchase
{
    [Table("DebitMemo", Schema = Schema.Purchasing)]
    [Index(nameof(TransCode))]
    public class DebitMemo : BaseEntityWithMark
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public short SrcTrans { get; set; }

        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }

        [StringLength(17)]
        public string TransCode { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Precision(18, 2)]
        public decimal Rate { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        [Precision(18, 2)]
        public decimal Used { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwDebitMemo : BaseEntityWithMark
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public short SrcTrans { get; set; }

        public string SupCode { get; set; }

        public string TransCode { get; set; }

        public string CurrCode { get; set; }

        [Precision(18, 2)]
        public decimal Rate { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        [Precision(18, 2)]
        public decimal Used { get; set; }

        public string Notes { get; set; }


        [Precision(19, 2)]
        public decimal Remaining { get; set; }

        public string SupInitial { get; set; }

        public string SupName { get; set; }

        public string SrcTransName { get; set; }

        public string Status { get; set; }
    }
}
