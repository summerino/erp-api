using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder
{
    public class OrderHeaderModel
    {
        public decimal Total { get; set; }
        public decimal TaxAmount { get; set; }
        public bool IncludeTax { get; set; }
        public decimal FinalDisc { get; set; }
        public decimal FinalDiscPercent { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Rate { get; set; }
        public decimal Dpp { get; set; }
        [Required]
        public string CurrCode { get; set; }
        public string CurrName { get; set; }
        [Required]
        public string CustCode { get; set; }
        public string CustName { get; set; }
        public short Type { get; set; }
        public string SalesOrderCode { get; set; }
        [Required]
        public string VisitLogCode { get; set; }
        public DateTime Date { get; set; }
        public string Code { get; set; }
        public int? PaymentTermId { get; set; }
        public string PaymentTermName { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
