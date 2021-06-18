using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Sales
{
    [Table("VisitOrder", Schema = Schema.Sales)]
    public class VisitOrder : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public long SalesmanId { get; set; }

        [StringLength(17)]
        public string VisitPlanCode { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    [Table("VisitOrderCustomer", Schema = Schema.Sales)]
    public class VisitOrderCustomer
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        [Required]
        [StringLength(8)]
        public string CustCode { get; set; }

        public long? ReplacingForSalesmanId { get; set; }

        public bool Visited { get; set; }
    }

    [Table("VisitOrderInvoice", Schema = Schema.Sales)]
    public class VisitOrderInvoice
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        [Required]
        [StringLength(17)]
        public string InvCode { get; set; }

        public bool Collecting { get; set; }

        public bool FailCollect { get; set; }

        [StringLength(256)]
        public string NotesFailCollect { get; set; }
    }

    public class VwVisitOrder : BaseEntityWithMarkAndApproved
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public long SalesmanId { get; set; }

        public string VisitPlanCode { get; set; }

        public string Notes { get; set; }

        public string CustCode { get; set; }

        public string CustomerName { get; set; }

        public string Status { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }
    }
}
