using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Sales
{
    [Table("PromoHeader", Schema = Schema.Sales)]
    public class PromoHeader : BaseEntityWithMarkApprovedAndViewed
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

        public short ApplyTo { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaCost { get; set; }

        public string Content { get; set; }
    }

    public class VwPromoHeader : BaseEntityWithMarkApprovedAndViewed
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public short ApplyTo { get; set; }

        public string CoaCost { get; set; }
        
        public string Content { get; set; }


        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string Status { get; set; }
    }

    [Table("PromoSubject", Schema = Schema.Sales)]
    public class PromoSubject
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        [StringLength(8)]
        public string CustCode { get; set; }

        public int? CustTypeId { get; set; }
    }
    
    [Table("PromoDetail", Schema = Schema.Sales)]
    [Index(nameof(ItemId))]
    public class PromoDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public short ApplyTo { get; set; }

        public int? ItemId { get; set; }

        public short PromoType { get; set; }

        public bool IsPercentage { get; set; }

        [Precision(5, 2)]
        public decimal ValuePercentage { get; set; }

        [Precision(18, 2)]
        public decimal ValueAmount { get; set; }

        public bool IsPromoWithBudget { get; set; }

        [Precision(18, 2)]
        public decimal BudgetMaximumValue { get; set; }

        public short OverBudgetAction { get; set; }

        [StringLength(50)]
        public string SubGroup1 { get; set; }

        [StringLength(50)]
        public string SubGroup2 { get; set; }

        [StringLength(50)]
        public string SubGroup3 { get; set; }

        [StringLength(50)]
        public string SubGroup4 { get; set; }

        [StringLength(50)]
        public string SubGroup5 { get; set; }
    }

    [Table("PromoDetailMultipleItem", Schema = Schema.Sales)]
    public class PromoDetailMultipleItem
    {
        public long Id { get; set; }

        public long PromoDetailId { get; set; }

        public int ItemId { get; set; }
    }

    [Table("PromoDetailTier", Schema = Schema.Sales)]
    public class PromoDetailTier
    {
        public long Id { get; set; }

        public long PromoDetailId { get; set; }

        [Precision(18, 2)]
        public decimal FromQty { get; set; }

        [Precision(18, 2)]
        public decimal? ToQty { get; set; }

        public bool IsPercentage { get; set; }

        [Precision(18, 2)]
        public decimal Value { get; set; }

        public int? SaleUnit { get; set; }

        public bool ApplyToAllUnit { get; set; }

        public int? FreeGoodItemId { get; set; }

        [StringLength(20)]
        public string UnitFreeGood { get; set; }

        public bool IsMultiple { get; set; }

        public int? PaymentTermId { get; set; }
    }
}
