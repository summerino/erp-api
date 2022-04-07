namespace ERP.Web.API.Domain.Models.Mobile.CustomerDeliverySchedule;

public class DeliveryScheduleHeaderModel
{
    public string Code { get; set; }
    public DateTime Date { get; set; }
    public int SrcTrans { get; set; }
    public string TransCode { get; set; }
    public string CustCode { get; set; }
    public string CustName { get; set; }
    public string WarehouseCode { get; set; }
    public string WarehouseName { get; set; }
    public long ShippedBy { get; set; }
    public decimal SubTotal { get; set; }
}