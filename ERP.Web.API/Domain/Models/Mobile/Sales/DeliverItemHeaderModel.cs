using System;

namespace ERP.Web.API.Domain.Models.Mobile.Sales
{
    public class DeliverItemHeaderModel
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public string DeliveryCode { get; set; }
        public string TransCode { get; set; }
        public short SrcTrans { get; set; }
        public string SupCode { get; set; }
        public int VehicleId { get; set; }
        public long DeliverBy { get; set; }
    }
}
