using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales
{
    [Table("MobileItemRequestHeader", Schema = Schema.MobileSales)]
    public class MobileItemRequestHeader : BaseEntityWithMarkApprovedAndRejected
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [StringLength(17)]
        public string TransferCode { get; set; }

        public long SalesmanId { get; set; }

        public int? AreaId1 { get; set; }

        public int? AreaId2 { get; set; }

        public int? AreaId3 { get; set; }

        public int? AreaId4 { get; set; }

        public int? AreaId5 { get; set; }
    }

    [Table("MobileItemRequestDetail", Schema = Schema.MobileSales)]
    public class MobileItemRequestDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        public int UnitId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Qty { get; set; }
    }

    public class VwMobileItemRequestHeader : BaseEntityWithMarkApprovedAndRejected
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public string TransferCode { get; set; }

        public long SalesmanId { get; set; }

        public int? AreaId1 { get; set; }

        public int? AreaId2 { get; set; }

        public int? AreaId3 { get; set; }

        public int? AreaId4 { get; set; }

        public int? AreaId5 { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string RejectedInitial { get; set; }

        public string SalesmanInitial { get; set; }

        public string SalesmanName { get; set; }

        public string Status { get; set; }
    }

    public class VwMobileItemRequestDetail
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        public int UnitId { get; set; }

        public decimal Qty { get; set; }

        public string ItemName { get; set; }

        public string UnitName { get; set; }

        public int UomId { get; set; }

    }
}
