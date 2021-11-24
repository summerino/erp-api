using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.MobileWarehouse
{
    [Table("MobileReceiveItemHeader", Schema = Schema.MobileWarehouse)]
    [Index(nameof(TransCode))]
    public class MobileReceiveItemHeader : BaseEntityWithMarkApprovedAndRejected
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [StringLength(17)]
        public string RcvCode { get; set; }

        [Required]
        [StringLength(17)]
        public string TransCode { get; set; }

        public short SrcTrans { get; set; }

        [Required]
        [StringLength(8)]
        public string SupCode { get; set; }

        public long ReceiveBy { get; set; }

        [Required]
        public string SignatureImage { get; set; }
    }

    public class VwMobileReceiveItemHeader : BaseEntityWithMarkApprovedAndRejected
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public string RcvCode { get; set; }

        public string TransCode { get; set; }

        public short SrcTrans { get; set; }

        public string SupCode { get; set; }

        public long ReceiveBy { get; set; }
        
        public string SignatureImage { get; set; }


        public string SupInitial { get; set; }

        public string ReceiveInitial { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string RejectedInitial { get; set; }

        public string Status { get; set; }
    }

    [Table("MobileReceiveItemDetail", Schema = Schema.MobileWarehouse)]
    public class MobileReceiveItemDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public long? TransDetailId { get; set; }

        public int ItemId { get; set; }

        [Precision(18, 2)]
        public decimal Qty { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        [Required]
        [StringLength(8)]
        public string WarehouseCode { get; set; }

        public int Type { get; set; }
    }

    public class VwMobileReceiveItemDetail
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public short LineNo { get; set; }

        public long? TransDetailId { get; set; }

        public int ItemId { get; set; }

        public decimal Qty { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        public string WarehouseCode { get; set; }

        public int Type { get; set; }


        public string ItemInitial { get; set; }

        public string ItemName { get; set; }

        public int? ItemUomBuyId { get; set; }

        public string ItemUomBuyName { get; set; }

        public decimal? ItemBuyPrice { get; set; }

        public string UomInitial { get; set; }

        public string UnitName { get; set; }
    }
}
