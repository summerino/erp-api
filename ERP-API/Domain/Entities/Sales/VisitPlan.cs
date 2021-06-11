using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Sales
{
    [Table("VisitPlanHeader", Schema = Schema.Sales)]
    public class VisitPlanHeader : BaseEntityWithMarkAndApproved
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public int GroupId { get; set; }
    }

    [Table("VisitPlanDetail", Schema = Schema.Sales)]
    public class VisitPlanDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public long SalesmanId { get; set; }
    }

    [Table("VisitPlanDetailCustomer", Schema = Schema.Sales)]
    public class VisitPlanDetailCustomer
    {
        public long Id { get; set; }

        public long VisitPlanDetailId { get; set; }

        [Required]
        [StringLength(8)]
        public string CustCode { get; set; }
    }
}
