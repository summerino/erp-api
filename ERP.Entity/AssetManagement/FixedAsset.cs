using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.AssetManagement
{
    [Table("FixedAsset", Schema = Schema.AssetManagement)]
    public class FixedAsset : BaseEntityWithMarkApprovedAndViewed
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public int TypeId { get; set; }

        [Column(TypeName = "date")]
        public DateTime PurchaseDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime StartDepreciateOn { get; set; }

        [Precision(18, 2)]
        public decimal PurchaseValue { get; set; }

        [Precision(18, 2)]
        public decimal AcquiredValue { get; set; }

        [Precision(18, 2)]
        public decimal SalvageValue { get; set; }

        public int DepreciationMonth { get; set; }

        public int DepartmentId { get; set; }

        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }

        [Required]
        [StringLength(17)]
        public string PurchaseOrderNo { get; set; }

        [Required]
        [StringLength(17)]
        public string InvoiceNo { get; set; }

        [Required]
        [StringLength(17)]
        public string PaymentVoucherNo { get; set; }

        public int YearWarranty { get; set; }

        [Required]
        [StringLength(17)]
        public string CodeWarranty { get; set; }

        public short EstimatedLife { get; set; }

        public int DepreciationMethod { get; set; }

        [Precision(18, 2)]
        public decimal InitDepreciationExpense { get; set; }

        [Precision(18, 2)]
        public decimal BookValue { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }

        [StringLength(6)]
        public string CoaExpense { get; set; }
    }

    public class VwFixedAsset : BaseEntityWithMarkApprovedAndViewed
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public int TypeId { get; set; }

        public DateTime PurchaseDate { get; set; }

        public DateTime StartDepreciateOn { get; set; }

        [Precision(18, 2)]
        public decimal PurchaseValue { get; set; }

        [Precision(18, 2)]
        public decimal AcquiredValue { get; set; }

        [Precision(18, 2)]
        public decimal SalvageValue { get; set; }

        public int DepreciationMonth { get; set; }

        public int DepartmentId { get; set; }

        public string SupCode { get; set; }

        public string PurchaseOrderNo { get; set; }

        public string InvoiceNo { get; set; }

        public string PaymentVoucherNo { get; set; }

        public int YearWarranty { get; set; }

        public string CodeWarranty { get; set; }

        public short EstimatedLife { get; set; }

        public int DepreciationMethod { get; set; }

        [Precision(18, 2)]
        public decimal InitDepreciationExpense { get; set; }

        [Precision(18, 2)]
        public decimal BookValue { get; set; }

        public string Notes { get; set; }

        public string CoaExpense { get; set; }


        public string SupName { get; set; }

        public string AssetType { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string Status { get; set; }
    }

    [Table("FixedAssetDepartment", Schema = Schema.AssetManagement)]
    public class FixedAssetDepartment
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public int DepartmentId { get; set; }
        
        [Precision(5, 2)]
        public decimal Percentage { get; set; }
    }

    [Table("FixedAssetHistory", Schema = Schema.AssetManagement)]
    public class FixedAssetHistory
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        [Required]
        [StringLength(17)]
        public string JournalCode { get; set; }

        [Required]
        [StringLength(4)]
        public string FiscalYear { get; set; }

        public short NumberOfMonth { get; set; }

        public short Period { get; set; }

        [Column(TypeName = "date")]
        public DateTime DepreciateDate { get; set; }

        [Precision(18, 2)]
        public decimal DepreciateValue { get; set; }

        [Precision(18, 2)]
        public decimal BookValue { get; set; }
    }

    [Table("AssetType", Schema = Schema.AssetManagement)]
    public class AssetType : BaseEntityWithActive
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Initial { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaDeprecExpense { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaAccumDeprec { get; set; }

        [Required]
        [StringLength(6)]
        public string CoaAsset { get; set; }

        [StringLength(6)]
        public string CoaExpense { get; set; }
    }

    public class VwAssetType : BaseEntityWithActive
    {
        public int Id { get; set; }

        public string Initial { get; set; }

        public string Name { get; set; }

        public string CoaDeprecExpense { get; set; }

        public string CoaAccumDeprec { get; set; }

        public string CoaAsset { get; set; }

        public string CoaExpense { get; set; }


        public string UpdatedInitial { get; set; }
    }
}
