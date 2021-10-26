namespace ERP.Web.API.Domain.Models.Mobile.Sales
{
    public class DeliverItemDetailModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public short LineNo { get; set; }
        public long? TransDetailId { get; set; }
        public int ItemId { get; set; }
        public decimal Qty { get; set; }
        public int UomId { get; set; }
        public int UnitId { get; set; }
        public string WarehouseCode { get; set; }
        public int Type { get; set; }
    }
}
