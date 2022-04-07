namespace ERP.Web.API.Domain.Models.Mobile.TransferStock;

public class MobileTransferStockDetailModel
{
    public int ItemId { get; set; }
    public int UomId { get; set; }
    public int UnitId { get; set; }
    public decimal OriginalQty { get; set; }
    public decimal RealizeQty { get; set; }
    public string ItemName { get; set; }
    public string UnitName { get; set; }
}