using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;

namespace ERP_API.Domain.Entities.Sales
{
    [Table("SalesmanGroup", Schema = Schema.Sales)]
    public class SalesmanGroup : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public long SupervisorId { get; set; }
    }

    public class VwSalesmanGroup : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public long SupervisorId { get; set; }


        public string SupervisorInitial { get; set; }

        public string UpdatedInitial { get; set; }
    }

    [Table("SalesmanSchedule", Schema = Schema.Sales)]
    public class SalesmanSchedule
    {
        public long Id { get; set; }

        public long SalesmanId { get; set; }

        public int? AreaId1 { get; set; }

        public int? AreaId2 { get; set; }

        public int? AreaId3 { get; set; }

        public int? AreaId4 { get; set; }

        public int? AreaId5 { get; set; }

        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EndDate { get; set; }

        public byte Recurrence { get; set; }

        public byte VisitDay { get; set; }
    }

    [Table("SalesmanScheduleCustomer", Schema = Schema.Sales)]
    public class SalesmanScheduleCustomer
    {
        public long Id { get; set; }

        public long SalesmanScheduleId { get; set; }

        [Required]
        [StringLength(8)]
        public string CustCode { get; set; }
    }
}
