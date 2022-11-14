using ERP.Common;
using ERP.Common.Extensions;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.HumanResource;
using ERP.Web.API.Domain.Interfaces.Mobile.HumanResource;
using ERP.Web.API.Domain.Models.Mobile.HumanResource;

namespace ERP.Web.API.Domain.Services.Mobile.HumanResource;

public class AttendanceService : IAttendanceService
{
    protected TenantContext Db;

    public AttendanceService(TenantContext db)
    {
        Db = db;
    }

    public DataSourceResult GetData(int skip, int take, IEnumerable<Filter> filter, IEnumerable<Sort> sort, int userId, string date)
    {
        var data_attendance = (from attendance in Db.Attendances
            join user in Db.Users on attendance.EmployeeId equals user.EmployeeId
            where user.Id.Equals(userId)
            select new Attendance
            {
                Id = attendance.Id,
                Date = attendance.Date,
                EmployeeId = user.Id,
                CheckIn = attendance.CheckIn,
                CheckInLat = attendance.CheckInLat,
                CheckInLng = attendance.CheckInLng,
                CheckInImage = attendance.CheckInImage,
                CheckInNotes = attendance.CheckInNotes,
                CheckOut = attendance.CheckOut,
                CheckOutLat = attendance.CheckOutLat,
                CheckOutLng = attendance.CheckOutLng,
                CheckOutImage = attendance.CheckOutImage,
                CheckOutNotes = attendance.CheckOutNotes,
                TotalHours = attendance.TotalHours
            }).AsQueryable();

        var year = DateTime.ParseExact(date, "yyyy-MM", null).Year;
        var month = DateTime.ParseExact(date, "yyyy-MM", null).Month;
        var day = DateTime.Now.Day;
        var selected_date = DateTime.DaysInMonth(year, month);

        if (month == DateTime.Now.Month && year == DateTime.Now.Year)
        {
            var data_date = Enumerable.Range(1, day)  // Days: 1, 2 ... 31 etc.
                .Select(day => new DateTime(year, month, day)) // Map each day to a date
                .ToList();

            var data = (from dd in data_date
                join attend in data_attendance on dd.Date.Date equals attend.Date.Date
                    into attendances
                from attend in attendances.DefaultIfEmpty()
                select new Attendance
                {
                    Id = attend?.Id ?? 0,
                    Date = dd.Date.Date,
                    EmployeeId = userId,
                    CheckIn = attend?.CheckIn,
                    CheckInLat = attend?.CheckInLat,
                    CheckInLng = attend?.CheckInLng,
                    CheckInImage = attend?.CheckInImage,
                    CheckInNotes = attend?.CheckInNotes,
                    CheckOut = attend?.CheckOut,
                    CheckOutLat = attend?.CheckOutLat,
                    CheckOutLng = attend?.CheckOutLng,
                    CheckOutImage = attend?.CheckOutImage,
                    CheckOutNotes = attend?.CheckOutNotes,
                    TotalHours = attend?.TotalHours
                }).AsQueryable();

            return data.ToDataSourceResult(skip, take, filter, sort);
        }
        else
        {
            var data_date = Enumerable.Range(1, selected_date)  // Days: 1, 2 ... 31 etc.
                .Select(day => new DateTime(year, month, day)) // Map each day to a date
                .ToList();

            var data = (from dd in data_date
                join attend in data_attendance on dd.Date.Date equals attend.Date.Date
                    into attendances
                from attend in attendances.DefaultIfEmpty()
                select new Attendance
                {
                    Id = attend?.Id ?? 0,
                    Date = dd.Date.Date,
                    EmployeeId = userId,
                    CheckIn = attend?.CheckIn,
                    CheckInLat = attend?.CheckInLat,
                    CheckInLng = attend?.CheckInLng,
                    CheckInImage = attend?.CheckInImage,
                    CheckInNotes = attend?.CheckInNotes,
                    CheckOut = attend?.CheckOut,
                    CheckOutLat = attend?.CheckOutLat,
                    CheckOutLng = attend?.CheckOutLng,
                    CheckOutImage = attend?.CheckOutImage,
                    CheckOutNotes = attend?.CheckOutNotes,
                    TotalHours = attend?.TotalHours
                }).AsQueryable();

            return data.ToDataSourceResult(skip, take, filter, sort);
        }
    }

    public SaveResult AddAttendance(AttendanceRequest data, int UserId)
    {
        var result = new SaveResult(false);

        using var transaction = Db.Database.BeginTransaction();
        try
        {
            long? employeeId = Db.Users.Where(x => x.Id.Equals(UserId)).Select(u => u.EmployeeId).SingleOrDefault();
            if (IsInitialExists(data.ClockTime.Date, employeeId, data.Type))
            {
                result.Message = "User sudah melakukan absensi.";
                return result;
            }

            var entity = Db.Attendances.FirstOrDefault(at => at.Date == data.ClockTime.Date && at.EmployeeId == employeeId);
            data.ClockTime = DateTime.Now;
            if (entity != null)
            {
                if (data.Type == "in")
                {
                    entity.CheckIn = data.ClockTime;
                    entity.CheckInImage = data.Image;
                    entity.CheckInLat = data.Latitude;
                    entity.CheckInLng = data.Longitude;
                    entity.CheckInNotes = data.Note;
                }
                else if (data.Type == "out")
                {
                    entity.CheckOut = data.ClockTime;
                    entity.CheckOutImage = data.Image;
                    entity.CheckOutLat = data.Latitude;
                    entity.CheckOutLng = data.Longitude;
                    entity.CheckOutNotes = data.Note;

                    var TotalHours = data.ClockTime.Subtract((DateTime)entity.CheckIn);
                    entity.TotalHours = (decimal?)TotalHours.TotalHours;

                }
                else
                {
                    result.Message = "tipe absensi salah.";
                    return result;
                }
            }
            else
            {
                Attendance newData = new();
                if (data.Type == "in")
                {
                    newData.Date = data.ClockTime.Date;
                    newData.CheckIn = data.ClockTime;
                    newData.CheckInLat = data.Latitude;
                    newData.CheckInLng = data.Longitude;
                    newData.CheckInImage = data.Image;
                    newData.CheckInNotes = data.Note;
                    newData.EmployeeId = (long)employeeId;
                }
                else if (data.Type == "out")
                {
                    newData.Date = data.ClockTime.Date;
                    newData.CheckOut = data.ClockTime;
                    newData.CheckOutLat = data.Latitude;
                    newData.CheckOutLng = data.Longitude;
                    newData.CheckOutImage = data.Image;
                    newData.CheckOutNotes = data.Note;
                    newData.EmployeeId = (long)employeeId;
                }

                Db.Attendances.Add(newData);
            }

            Db.SaveChanges();
            transaction.Commit();
        }
        catch (Exception ex)
        {
            result.Message = ex.InnerException?.Message ?? ex.Message;
            return result;
        }

        result.Success = true;
        result.Data = data;
        result.Message = "Data pelanggan berhasil disimpan.";
        return result;
    }

    private bool IsInitialExists(DateTime date, long? employeeId, string type)
    {
        if (employeeId != null)
        {
            if (type == "in")
                return Db.Attendances.Any(x => x.Date == date.Date && x.CheckIn != null && x.EmployeeId == employeeId);
            else
                return Db.Attendances.Any(x => x.Date == date.Date && x.CheckOut != null && x.EmployeeId == employeeId);
        }
        return false;
    }

    public Attendance GetCurrentAttendance(long userId)
    {
        var now = DateTime.Now.Date;

        var data = (from attend in Db.Attendances
            join user in Db.Users on attend.EmployeeId equals user.EmployeeId
            where attend.Date == now && user.Id == userId
            select new Attendance
            {
                Id = attend.Id,
                Date = attend.Date,
                EmployeeId = attend.EmployeeId,
                CheckIn = attend.CheckIn,
                CheckInLat = attend.CheckInLat,
                CheckInLng = attend.CheckInLng,
                CheckInImage = attend.CheckInImage,
                CheckInNotes = attend.CheckInNotes,
                CheckOut = attend.CheckOut,
                CheckOutLat = attend.CheckOutLat,
                CheckOutLng = attend.CheckOutLng,
                CheckOutImage = attend.CheckOutImage,
                CheckOutNotes = attend.CheckOutNotes,
                TotalHours = attend.TotalHours
            }).SingleOrDefault();
        return data;
    }

    public Attendance GetDetail(long attendanceId)
    {
        var data = (from attend in Db.Attendances
            where attend.Id == attendanceId
            select new Attendance
            {
                Id = attend.Id,
                Date = attend.Date,
                EmployeeId = attend.EmployeeId,
                CheckIn = attend.CheckIn,
                CheckInLat = attend.CheckInLat,
                CheckInLng = attend.CheckInLng,
                CheckInImage = attend.CheckInImage,
                CheckInNotes = attend.CheckInNotes,
                CheckOut = attend.CheckOut,
                CheckOutLat = attend.CheckOutLat,
                CheckOutLng = attend.CheckOutLng,
                CheckOutImage = attend.CheckOutImage,
                CheckOutNotes = attend.CheckOutNotes,
                TotalHours = attend.TotalHours
            }).SingleOrDefault();
        return data;
    }

    public DateTime? GetLastAttendance(int userId)
    {
        var employeeId = Db.Users.Where(x => x.Id.Equals(userId)).Select(x => x.EmployeeId).FirstOrDefault();
        var data = Db.Attendances.Where(z => z.EmployeeId.Equals(employeeId)).Select(y => y.Date).OrderByDescending(i => i.Date).FirstOrDefault();

        return data;
    }
}