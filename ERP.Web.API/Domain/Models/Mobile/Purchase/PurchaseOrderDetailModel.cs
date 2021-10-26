namespace ERP.Web.API.Domain.Models.Mobile.Purchase
{
    public class PurchaseOrderDetailModel
    {
        public string Code { get; set; }
        public int LineNo { get; set; }
        public int ItemId { get; set; }
        public decimal OrderQty { get; set; }
        public decimal? ReceiveQty { get; set; }
        public int TransDetailId { get; set; }
        public int Type { get; set; }
        public int UnitId { get; set; }
        public int UomId { get; set; }
        public string WarehouseCode { get; set; }
        public string ItemInitial { get; set; }
        public string ItemName { get; set; }
        public string UnitName { get; set; }
    }
}
