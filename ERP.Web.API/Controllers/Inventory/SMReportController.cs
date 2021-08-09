using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.Inventory;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace ERP.Web.API.Controllers.Inventory
{
    [Route("sm-report")]
    [ApiController]
    public class SMReportController : ControllerBase
    {
        private readonly ISMReportService _sm;
        public SMReportController(ISMReportService sm)
        {
            _sm = sm;
        }

        [HttpGet]
        public IActionResult GetData(int type, string startDate, string endDate, string whCode, int itemId, int typeUnit, bool isSM, string sorts)
        {
            var result = _sm.GetData(type, startDate, endDate, whCode, itemId, typeUnit, isSM, JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
