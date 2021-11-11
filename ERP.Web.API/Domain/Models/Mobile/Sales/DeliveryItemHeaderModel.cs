using System;

namespace ERP.Web.API.Domain.Models.Mobile.Sales
{
    public class DeliveryItemHeaderModel
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public string DlvPlanCode { get; set; }
        public DateTime DlvPlanDate { get; set; }
        public long DriverId { get; set; }
        public string DriverName { get; set; }
        public int VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string Notes { get; set; }
    }
}
