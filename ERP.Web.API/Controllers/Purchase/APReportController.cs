using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.Purchase;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Purchase
{
    [Route("ap-report")]
    [ApiController]
    public class APReportController : ControllerBase
    {
        private readonly IAPReportService _ap;
        public APReportController(IAPReportService ap)
        {
            _ap = ap;
        }

        [HttpGet]
        public IActionResult GetData(int type, string supCode, string date, string sorts)
        {
            var result = _ap.GetData(type, date, supCode, JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
