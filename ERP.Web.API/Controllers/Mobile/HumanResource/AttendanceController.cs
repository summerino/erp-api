using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Entity;
using ERP.Entity.General;
using ERP.Entity.HumanResource;
using ERP.Web.API.Domain.Interfaces.Mobile.HumanResource;
using ERP.Web.API.Domain.Models.Mobile.HumanResource;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Mobile.HumanResource;

[Authorize(AppConstant.ValidateMobileTokenPolicy)]
[Route("mobile/[controller]")]
[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendance;
    private readonly IClaimService _claim;

    public AttendanceController(IAttendanceService attendance, IClaimService claim)
    {
        _attendance = attendance;
        _claim = claim;
    }

    [HttpGet]
    public IActionResult GetData(string filters, string sorts, int skip, int take, string date)
    {
        var data =
            _attendance.GetData(skip, take,
                JsonConvert.DeserializeObject<List<Filter>>(!string.IsNullOrWhiteSpace(filters) ? filters : "[]"),
                JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"), _claim.UserId, date);


        var result = ((List<Attendance>)data.Data).ToList<dynamic>();

        return Ok(new MobileApiResponse
        {
            Count = data.Total,
            Data = result
        }); ;
    }

    [HttpPost("add")]
    public IActionResult AddAttendance(AttendanceRequest data)
    {
        var result = _attendance.AddAttendance(data, _claim.UserId);

        return Ok(result);
    }

    [HttpGet("current")]
    public IActionResult GetCurrentAttendance()
    {
        var result = _attendance.GetCurrentAttendance(_claim.UserId);
            
        return Ok(result);
    }

    [HttpGet("detail")]
    public IActionResult GetDetail(long attendanceId)
    {
        var result = _attendance.GetDetail(attendanceId);

        return Ok(result);
    }

    [HttpGet("last-attendance")]
    public IActionResult GetLastAttendance()
    {
        var result = _attendance.GetLastAttendance(_claim.UserId);

        return Ok(result);
    }
}