using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Purchase
{
    [Table("DebitMemo", Schema = Schema.Purchasing)]
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

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Used { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }
}
