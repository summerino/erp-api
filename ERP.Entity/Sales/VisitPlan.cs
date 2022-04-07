using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.Sales;

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

public class VwVisitPlanHeader : BaseEntityWithMarkAndApproved
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public int GroupId { get; set; }

    public string GroupInitial { get; set; }

    public string GroupName { get; set; }

    public string SupervisorName { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string ApprovedInitial { get; set; }

    public string Status { get; set; }
}

public class VwVisitPlanDetail
{
    public long VisitPlanDetailId { get; set; }

    public string Code { get; set; }

    public long Id { get; set; }

    public string Initial { get; set; }

    public short Type { get; set; }

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


    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public byte? Recurrence { get; set; }

    public byte? VisitDay { get; set; }

    public string FullName { get; set; }

    public string GroupInitial { get; set; }

    public string GroupNameInitial { get; set; }

    public string GenderInitial { get; set; }
}

public class VwVisitPlanDetailCustomer
{
    public long VisitPlanDetailId { get; set; }

    public string Code { get; set; }

    public string Initial { get; set; }

    public string Name { get; set; }

    public string AreaName1 { get; set; }

    public string AreaName2 { get; set; }

    public string AreaName3 { get; set; }

    public string AreaName4 { get; set; }

    public string AreaName5 { get; set; }

    public string Email { get; set; }

    public string Website { get; set; }

    public int TypeId { get; set; }
}