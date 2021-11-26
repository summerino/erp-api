namespace ERP.Web.API.Domain.Models.Mobile.CustomerDeliverySchedule
{
    public class DeliveryScheduleDetailModel
    {
        public string Code { get; set; }
        public int ItemId { get; set; }
        public string ItemInitial { get; set; }
        public string ItemName { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public int UomId { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
