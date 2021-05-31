using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP_API.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Domain.Entities.Sales
{
    [Table("DeliveryPlanHeader", Schema = Schema.Sales)]
    public class DeliveryPlanHeader : BaseEntityWithMarkAndApproved
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

        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalVolume { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal TotalWeight { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalVehicleVolume { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalVehicleWeight { get; set; }

        [StringLength(256)]
        public string Notes { get; set; }
    }

    public class VwDeliveryPlanHeader : BaseEntityWithMarkAndApproved
    {
        public string Code { get; set; }

        public DateTime Date { get; set; }

        public short SrcTrans { get; set; }

        public int VehicleId { get; set; }

        public long DriverId { get; set; }

        public string WarehouseCode { get; set; }

        public decimal TotalVolume { get; set; }

        public decimal TotalWeight { get; set; }

        public decimal TotalVehicleVolume { get; set; }

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

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Volume { get; set; }

        [Column(TypeName = "decimal(19, 6)")]
        public decimal Weight { get; set; }

        public short SrcTrans { get; set; }

        public bool IsFailShipment { get; set; }

        [StringLength(256)]
        public string NotesFailShipment { get; set; }
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

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Qty { get; set; }

        [StringLength(8)]
        public string WarehouseCode { get; set; }

        public int Type { get; set; }
    }
}
