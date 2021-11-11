using System;

namespace ERP.Web.API.Domain.Models.Mobile.Sales
{
    public class DeliveryPlanHeaderModel
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public int srcTrans { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public int VehicleId { get; set; }
        public string Vehicle { get; set; }
        public long DriverId { get; set; }
        public string DriverName { get; set; }
        public string Notes { get; set; }
        public string Mark { get; set; }
    }
}