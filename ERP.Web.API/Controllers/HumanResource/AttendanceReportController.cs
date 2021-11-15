using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.HumanResource;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.HumanResource
{
    [Route("attendance-report")]
    [ApiController]
    public class AttendanceReportController : ControllerBase
    {
        private readonly IAttendanceReportService _attendance;

        public AttendanceReportController(IAttendanceReportService attendance)
        {
            _attendance = attendance;
        }

        [HttpGet]
        public IActionResult GetData(string startDate, string endDate, short employeeType, int salesGroupId, string employee, string sorts)
        {
            var result =
                _attendance.GetData(startDate, endDate, employeeType, salesGroupId, employee,
                    JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));
            
            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
