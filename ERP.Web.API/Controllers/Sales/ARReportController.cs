using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.Sales;
using ERP.Web.API.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;

namespace ERP.Web.API.Controllers.Sales
{
    [Route("ar-report")]
    [ApiController]
    public class ArReportController : ControllerBase
    {
        private readonly IARReportService _ar;
        public ArReportController(IARReportService ar)
        {
            _ar = ar;
        }

        [HttpGet]
        public IActionResult GetData(int type, string custCode, int slsId, string date, string sorts)
        {
            var result = _ar.GetData(type, date, custCode, slsId, JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
