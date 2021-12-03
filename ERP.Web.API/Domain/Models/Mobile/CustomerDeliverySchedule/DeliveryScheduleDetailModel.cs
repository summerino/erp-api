namespace ERP.Web.API.Domain.Models.Mobile.CustomerDeliverySchedule
{
    public class DeliveryScheduleDetailModel
    {
        public string Code { get; set; }
        public short LineNo { get; set; }
        public long? SoDetailId { get; set; }
        public int ItemId { get; set; }
        public decimal Qty { get; set; }
        public int UomId { get; set; }
        public int UnitId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Disc { get; set; }
        //public decimal TaxAmount { get; set; }
        public decimal NettPrice { get; set; }
        public decimal Total { get; set; }
        public decimal OrderQty { get; set; }
        public decimal OutstandingQty { get; set; }
        public string ItemInitial { get; set; }
        public string ItemName { get; set; }
        public string UnitName { get; set; }
    }
}
