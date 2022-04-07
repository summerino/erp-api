namespace ERP.Web.API.Domain.Models.Mobile.TransactionHistory;

public class TransactionHistoryByUnitProduct
{
    public long SalesId { get; set; }
    public DateTime Date { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public decimal Quantity { get; set; }
    public int UomId { get; set; }
    public int? UomToConvertId { get; set; }
    public int UnitId { get; set; }
    public string Unit { get; set; }
    public int CurrentSeq { get; set; }
    public int BaseSeq { get; set; }
    public decimal Conversion { get; set; }
    public decimal Total { get; set; }
}