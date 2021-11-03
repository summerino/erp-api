using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Entity.Core;

namespace ERP.Entity.MobileWarehouse
{
    [Table("MobileTransferStockHeader", Schema = Schema.MobileWarehouse)]
    public class MobileTransferStockHeader : BaseEntityWithMarkApprovedAndRejected
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Required]
        [StringLength(17)]
        public string TransferCode { get; set; }

        [Required]
        public string SignatureImage { get; set; }
    }

    public class VwMobileTransferStockHeader : BaseEntityWithMarkApprovedAndRejected
    {
        public string Code { get; set; }

        public string TransferCode { get; set; }
        
        public string SignatureImage { get; set; }


        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string RejectedInitial { get; set; }

        public string Status { get; set; }
    }

    [Table("MobileTransferStockDetail", Schema = Schema.MobileWarehouse)]
    public class MobileTransferStockDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal OriginalQty { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal RealizeQty { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }
    }

    public class VwMobileTransferStockDetail
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        public decimal OriginalQty { get; set; }

        public decimal RealizeQty { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }


        public string ItemInitial { get; set; }

        public string ItemName { get; set; }

        public int? ItemUomSellId { get; set; }

        public string ItemUomSellName { get; set; }

        public decimal? ItemSellPrice { get; set; }

        public string UomInitial { get; set; }

        public string UnitName { get; set; }
    }
}
