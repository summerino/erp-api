namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder;

public class VisitOrderModel
{
    public string Code { get; set; }
    public DateTime Date { get; set; }
    public string CustomerId { get; set; }
    public string CustomerName { get; set; }
    public string Status { get; set; }
    public string Notes { get; set; }
    public DateTime VisitTime { get; set; }
    public DateTime UpdatedDate { get; set; }
}