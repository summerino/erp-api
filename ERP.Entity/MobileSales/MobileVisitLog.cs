using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales;

[Table("MobileVisitLog", Schema = Schema.MobileSales)]
[Index(nameof(CustCode))]
public class MobileVisitLog : BaseEntityWithMarkApprovedAndRejected
{
    [Key]
    [StringLength(17)]
    public string Code { get; set; }

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [StringLength(17)]
    public string VisitOrderCode { get; set; }

    public long SalesmanId { get; set; }

    [Required]
    [StringLength(8)]
    public string CustCode { get; set; }

    public bool Scheduled { get; set; }

    public bool? Visited { get; set; }

    [Precision(9, 6)]
    public decimal? Lat { get; set; }

    [Precision(9, 6)]
    public decimal? Lng { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartTime { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndTime { get; set; }

    [Precision(18, 2)]
    public decimal? Total { get; set; }

    public int? UnscheduledVisitReasonId { get; set; }

    public int? NoVisitReasonId { get; set; }

    public int? NoOrderReasonId { get; set; }

    public string Image { get; set; }
}

[Table("MobileVisitReason", Schema = Schema.MobileSales)]
public class MobileVisitReason
{
    public long Id { get; set; }

    [Required]
    [StringLength(17)]
    public string VisitLogCode { get; set; }

    public int VisitReasonId { get; set; }
}

public class VwMobileVisitLog : BaseEntityWithMarkApprovedAndRejected
{
    public string Code { get; set; }

    public DateTime Date { get; set; }

    public string VisitOrderCode { get; set; }

    public long SalesmanId { get; set; }

    public string CustCode { get; set; }

    public bool Scheduled { get; set; }

    public bool? Visited { get; set; }

    [Precision(9, 6)]
    public decimal? Lat { get; set; }

    [Precision(9, 6)]
    public decimal? Lng { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [Precision(18, 2)]
    public decimal? Total { get; set; }

    public int? UnscheduledVisitReasonId { get; set; }

    public int? NoVisitReasonId { get; set; }

    public int? NoOrderReasonId { get; set; }

    public string Image { get; set; }

    public string CreatedInitial { get; set; }

    public string UpdatedInitial { get; set; }

    public string ApprovedInitial { get; set; }

    public string RejectedInitial { get; set; }

    public string SalesmanInitial { get; set; }

    public string SalesmanName { get; set; }

    public string CustomerInitial { get; set; }

    public string CustomerName { get; set; }

    public string Status { get; set; }
}