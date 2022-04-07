namespace ERP.Web.API.Domain.Models.Mobile.Purchase;

public class ReceiveItemDetailModel
{
    public long Id { get; set; }
    public string Code { get; set; }
    public short LineNo { get; set; }
    public long? TransDetailId { get; set; }
    public int ItemId { get; set; }
    public string ItemInitial { get; set; }
    public string ItemName { get; set; }
    public decimal Qty { get; set; }
    public decimal? QtyOrder { get; set; }
    public decimal? QtyRemain { get; set; }
    public int UomId { get; set; }
    public int UnitId { get; set; }
    public string UnitEquivalent { get; set; }
    public string WarehouseCode { get; set; }
    public int Type { get; set; }
}