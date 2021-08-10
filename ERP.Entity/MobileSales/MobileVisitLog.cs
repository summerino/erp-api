using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales
{
    [Table("MobileVisitLog", Schema = Schema.MobileSales)]
    [Index(nameof(CustCode))]
    public class MobileVisitLog : BaseEntityWithMarkAndApproved
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

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? Lat { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal? Lng { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? StartTime { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? EndTime { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
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
}
