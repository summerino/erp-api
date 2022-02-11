using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERP.Entity.Core;

namespace ERP.Entity.Sales
{
    [Table("DeliveryPlanHeader", Schema = Schema.Sales)]
    public class DeliveryPlanHeader : BaseEntityWithMarkApprovedAndViewed
    {
        [Key]
        [StringLength(17)]
        public string Code { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public short SrcTrans { get; set; }

        public int VehicleId { get; set; }

        public long DriverId { get; set; }

        [Required]
        [StringLength(8)]
        public string WarehouseCode { get; set; }

        [Precision(19, 6)]
        public decimal TotalVolume { get; set; }

        [Precision(19, 6)]
        public decimal TotalWeight { get; set; }

        [Precision(18, 2)]
        public decimal TotalVehicleVolume { get; set; }

        [Precision(18, 2)]
        public decimal TotalVehicleWeight { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwDeliveryPlanHeader : BaseEntityWithMarkApprovedAndViewed
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public short SrcTrans { get; set; }

        public int VehicleId { get; set; }

        public long DriverId { get; set; }

        public string WarehouseCode { get; set; }

        [Precision(19, 6)]
        public decimal TotalVolume { get; set; }

        [Precision(19, 6)]
        public decimal TotalWeight { get; set; }

        [Precision(18, 2)]
        public decimal TotalVehicleVolume { get; set; }

        [Precision(18, 2)]
        public decimal TotalVehicleWeight { get; set; }

        public string Notes { get; set; }


        public string VehicleNo { get; set; }

        public string DriverInitial { get; set; }

        public string WarehouseInitial { get; set; }

        public string CreatedInitial { get; set; }

        public string UpdatedInitial { get; set; }

        public string ApprovedInitial { get; set; }

        public string Status { get; set; }
    }

    [Table("DeliveryPlanDetail", Schema = Schema.Sales)]
    [Index(nameof(TransCode))]
    public class DeliveryPlanDetail
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public short LineNo { get; set; }

        [Required]
        [StringLength(17)]
        public string TransCode { get; set; }

        [Precision(19, 6)]
        public decimal Volume { get; set; }

        [Precision(19, 6)]
        public decimal Weight { get; set; }

        public short SrcTrans { get; set; }

        public bool IsFailShipment { get; set; }

        public bool FailedSendAll { get; set; }

        [StringLength(256)]
        public string NotesFailShipment { get; set; }
    }

    [Table("DeliveryPlanDetailItem", Schema = Schema.Sales)]
    [Index(nameof(TransDetailId))]
    public class DeliveryPlanDetailItem
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public long DlvPlanDetailId { get; set; }

        public short LineNo { get; set; }

        public long TransDetailId { get; set; }

        public int ItemId { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        [Precision(19, 6)]
        public decimal Qty { get; set; }
        
        public int Type { get; set; }
    }

    [Table("DeliveryPlanUndeliveredItem", Schema = Schema.Sales)]
    public class DeliveryPlanUndeliveredItem
    {
        public long Id { get; set; }

        [StringLength(17)]
        public string Code { get; set; }

        public long DlvPlanDetailId { get; set; }

        public short LineNo { get; set; }

        public int ItemId { get; set; }

        public int UomId { get; set; }

        public int UnitId { get; set; }

        [Precision(18, 2)]
        public decimal Qty { get; set; }

        [StringLength(8)]
        public string WarehouseCode { get; set; }

        public int Type { get; set; }
    }
}
