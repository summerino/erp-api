using ERP.Common;
using ERP.Common.Models;
using ERP.Entity.General;
using ERP.Entity.HumanResource;
using ERP.Web.API.Domain.Models.Mobile.HumanResource;

namespace ERP.Web.API.Domain.Interfaces.Mobile.HumanResource;

public interface IAttendanceService
{
    DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filters, IEnumerable<Sort> sorts, int userId, string date);

    SaveResult AddAttendance(AttendanceRequest data, int UserId);

    Attendance GetCurrentAttendance(long userId);

    Attendance GetDetail(long attendanceId);
    DateTime? GetLastAttendance(int userId);
}