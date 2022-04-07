namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder;

public class VisitLogDetailModel
{
    public string VisitOrderCode { get; set; }
    public DateTime Date { get; set; }
    public string Code { get; set; }
    public string CustCode { get; set; }
    public string CustInitial { get; set; }
    public string CustName { get; set; }
    public bool Scheduled { get; set; }
    public bool? Visited { get; set; }
    public decimal? Lat { get; set; }
    public decimal? Lng { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? Total { get; set; }
    public string Image { get; set; }
    public int? NoOrderReasonId { get; set; }
    public string NoOrderReasonName { get; set; }
    public int? NoVisitReasonId { get; set; }
    public string NoVisitReasonName { get; set; }
    public int? UnscheduledVisitReasonId { get; set; }
    public string UnscheduledVisitReasonName { get; set; }
    public bool OnGoing { get; set;}
    public bool IsDraft { get; set; }
}