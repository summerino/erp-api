namespace ERP.Web.API.Domain.Models.Mobile.Sales
{
    public class DeliveryPlanDetailModel
    {
        public string Code { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal QtyOrder { get; set; }
        public decimal QtyLoad { get; set; }
        public int UnitId { get; set; }
        public string UnitEquivalent { get; set; }
    }
}
