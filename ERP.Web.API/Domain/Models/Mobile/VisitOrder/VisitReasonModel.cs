namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder;

public class VisitReasonModel
{
    public long Id { get; set; }
    public string VisitLogCode { get; set; }
    public int VisitReasonId { get; set; }
    public string VisitReasonName { get; set; }
}