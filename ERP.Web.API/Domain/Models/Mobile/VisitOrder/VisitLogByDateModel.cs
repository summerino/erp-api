namespace ERP.Web.API.Domain.Models.Mobile.VisitOrder
{
    public class VisitLogByDateModel
    {
        public string Code { get; set; }
        public DateTime Date { get; set; }
        public string CustCode { get; set; }
        public string CustInitial { get; set; }
        public string CustName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool? Visited { get; set; }
        public bool Scheduled { get; set; }
        public int? UnscheduledVisitReasonId { get; set; }
        public int? AreaId1 { get; set; }
        public int? AreaId2 { get; set; }
        public int? AreaId3 { get; set; }
        public int? AreaId4 { get; set; }
        public int? AreaId5 { get; set; }
        public string AreaName1 { get; set; }
        public string AreaName2 { get; set; }
        public string AreaName3 { get; set; }
        public string AreaName4 { get; set; }
        public string AreaName5 { get; set; }

    }
}
