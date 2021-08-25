using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.HumanResources;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.HumanResources
{
    [Route("attendance")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendance;

        public AttendanceController(IAttendanceService attendance)
        {
            _attendance = attendance;
        }
        [HttpGet]
        [Route("GetDataReport")]
        public IActionResult GetDataReport(string startDate, string endDate, short employeeType, int salesGroupId, string employee, string sorts)
        {
            var result = _attendance.GetDataReport(startDate, endDate, employeeType, salesGroupId, employee, JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));
            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
