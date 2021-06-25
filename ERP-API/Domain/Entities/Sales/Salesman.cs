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

    public class VwSalesmanSchedule : BaseEntityWithActive
    {
        public long SalesmanScheduleId { get; set; }

        public long Id { get; set; }

        public string Initial { get; set; }

        public short Type { get; set; }

        public int? SalesGroupId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool Sex { get; set; }

        public DateTime? BirthDate { get; set; }

        public string BirthPlace { get; set; }

        public byte? MaritalStatus { get; set; }

        public string IdentityCardNo { get; set; }

        public byte Religion { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Phone { get; set; }

        public int? AreaId1 { get; set; }
                          
        public int? AreaId2 { get; set; }
                          
        public int? AreaId3 { get; set; }
                          
        public int? AreaId4 { get; set; }
                          
        public int? AreaId5 { get; set; }

        public string AreaName1 { get; set; }

        public string AreaName2 { get; set; }

        public string AreaName3 { get; set; }

        public string AreaName4 { get; set; }

        public string AreaName5 { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public byte? Recurrence { get; set; }

        public byte? VisitDay { get; set; }

        public string FullName { get; set; }
    }

    public class VwSalesmanScheduleCustomer : BaseEntityWithActive
    {
        public long SalesmanScheduleId { get; set; }

        public string Code { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public int TypeId { get; set; }

        public string Email { get; set; }

        public string Website { get; set; }

        public short CreditTerm { get; set; }

        public decimal CreditLimit { get; set; }

        public string RefNo { get; set; }

        public string Notes { get; set; }

        public int? BillingAddressId { get; set; }

        public int? ShippingAddressId { get; set; }

        public int? AreaId1 { get; set; }

        public int? AreaId2 { get; set; }

        public int? AreaId3 { get; set; }

        public int? AreaId4 { get; set; }

        public int? AreaId5 { get; set; }

        public string AreaName1 { get; set; }

        public string AreaName2 { get; set; }

        public string AreaName3 { get; set; }

        public string AreaName4 { get; set; }

        public string AreaName5 { get; set; }
    }
}
