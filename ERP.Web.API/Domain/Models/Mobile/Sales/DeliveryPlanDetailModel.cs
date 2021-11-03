namespace ERP.Web.API.Domain.Models.Mobile.Sales
{
    public class DeliveryPlanDetailModel
    {
        public string Code { get; set; }
        public short LineNo { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemInitial { get; set; }
        public decimal OrderQty { get; set; }
        public decimal LoadQty { get; set; }
        public int UomId { get; set; }
        public int UnitId { get; set; }
        public string UnitEquivalent { get; set; }
        public string TransCode { get; set; }
    }
}
