using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.MobileSales
{
    [Table("MobileItemRequestHeader", Schema = Schema.MobileSales)]
    public class MobileItemRequestHeader : BaseEntityWithMarkAndApproved
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
}
