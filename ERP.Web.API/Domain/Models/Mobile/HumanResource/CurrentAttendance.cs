namespace ERP.Web.API.Domain.Models.Mobile.HumanResource;

public class CurrentAttendance
{
    public DateTime CurrentDate { get; set; }
    public DateTime? ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
}