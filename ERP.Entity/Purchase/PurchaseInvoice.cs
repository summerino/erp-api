using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Purchase
{
    [Table("PurchaseInvoiceHeader", Schema = Schema.Purchasing)]
    public class PurchaseInvoiceHeader : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "date")]
        public DateTime DueDate { get; set; }

        [Column("POCode")]
        [StringLength(17)]
        public string PoCode { get; set; }

        [StringLength(30)]
        public string RefNo { get; set; }

        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }

        public long IssuedBy { get; set; }

        [Required]
        [StringLength(3)]
        public string CurrCode { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Total { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwPurchaseInvoiceHeader : BaseEntityWithMarkAndApproved
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public DateTime DueDate { get; set; }

        public string PoCode { get; set; }

        public string RefNo { get; set; }

        public string SupCode { get; set; }

        public long IssuedBy { get; set; }

        public string CurrCode { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal Total { get; set; }
        public decimal Remaining { get; set; }

        public string Notes { get; set; }


        public string SupName { get; set; }

        public string IssuedInitial { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string Status { get; set; }
    }

    [Table("PurchaseInvoiceDetail", Schema = Schema.Purchasing)]
    public class PurchaseInvoiceDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        [Required]
        [StringLength(17)]
        public string RcvCode { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ShipmentFee { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal HandlingFee { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal FinalDisc { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Total { get; set; }

        [Column("DPP", TypeName = "decimal(19, 6)")]
        public decimal Dpp { get; set; }
    }

    [Table("PurchaseInvoiceDebitMemo", Schema = Schema.Purchasing)]
    public class PurchaseInvoiceDebitMemo
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string InvCode { get; set; }

        [Required]
        [StringLength(17)]
        public string DebitMemoCode { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal InvAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DebitMemoAmount { get; set; }
    }
}
