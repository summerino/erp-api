namespace ERP.Web.API.Domain.Models.Mobile.Purchase
{
    public class PurchaseOrderDetailModel
    {
        //public PurchaseOrderDetailModel();
        public string Code { get; set; }
        public int ItemId { get; set; }
        public string ItemInitial { get; set; }
        public string ItemName { get; set; }
        public decimal? ReceiveQty { get; set; }
        public int UomId { get; set; }
        public string Uom { get; set; }
        public decimal OrderQty { get; set; }
        public decimal? RemainQty { get; set; }
        public short LineNo { get; set; }
        //public decimal NettPrice { get; set; }
        //public decimal Total { get; set; }
        //public decimal Dpp { get; set; }
        //[StringLength(256)]
        //public string Notes { get; set; }
        //[StringLength(6)]
        //public string CoaInventory { get; set; }
        //[StringLength(6)]
        //public string CoaCogs { get; set; }
        //public decimal TaxAmount { get; set; }
        //[StringLength(6)]
        //public string CoaPurc { get; set; }
        //[StringLength(6)]
        //public string CoaPurcReturn { get; set; }
        //public int Type { get; set; }
        //public string ItemName { get; set; }
        //public int? ItemUomBuyId { get; set; }
        //public string ItemUomBuyName { get; set; }
        //public decimal? ItemBuyPrice { get; set; }
        //[StringLength(6)]
        //public string CoaPurcDisc { get; set; }
        //public string UomInitial { get; set; }
        //public int? TaxId { get; set; }
        //public decimal UnitPrice { get; set; }
        //public long Id { get; set; }
        //[StringLength(17)]
        //public string Code { get; set; }
        //public short LineNo { get; set; }
        //public int ItemId { get; set; }
        //public int UomId { get; set; }
        //public int UnitId { get; set; }
        //public decimal Disc { get; set; }
        //public decimal Qty { get; set; }
        //public decimal? Width { get; set; }
        //public decimal? Height { get; set; }
        //public decimal? Weight { get; set; }
        //[StringLength(10)]
        //public string DimensionMeasurement { get; set; }
        //[StringLength(10)]
        //public string WeightMeasurement { get; set; }
        //public decimal? QtyRcv { get; set; }
        //public decimal? Length { get; set; }
        //public string UnitName { get; set; }
    }
}
