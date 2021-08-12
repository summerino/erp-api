using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.Finance;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Finance
{
    [Route("cb-report")]
    [ApiController]
    public class CBReportController : ControllerBase
    {
        private readonly ICBReportService _cb;
        public CBReportController(ICBReportService cb)
        {
            _cb = cb;
        }
        [HttpGet]
        public IActionResult GetData(int? type, string startDate, string endDate, string coaCode, string sorts)
        {
            var result = _cb.GetData(type, startDate, endDate, coaCode, JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
