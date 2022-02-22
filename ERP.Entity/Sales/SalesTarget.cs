using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Sales
{
    [Table("SalesTargetHeader", Schema = Schema.Sales)]
    public class SalesTargetHeader : BaseEntityWithMark
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }
    }

    public class VwSalesTargetHeader : BaseEntityWithMark
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }


        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string Status { get; set; }
    }

    [Table("SalesTargetSubject", Schema = Schema.Sales)]
    public class SalesTargetSubject
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public long SalesmanId { get; set; }
    }
    
    [Table("SalesTargetDetail", Schema = Schema.Sales)]
    [Index(nameof(SubGroup))]
    public class SalesTargetDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemGroupId { get; set; }

        public int ItemSubGroupId { get; set; }

        [Required]
        [StringLength(50)]
        public string SubGroup { get; set; }

        [Precision(18, 2)]
        public decimal Amount { get; set; }
    }
}
