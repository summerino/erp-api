using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using ERP.Common.Models;
using ERP.Web.API.Domain.Interfaces.Expedition;
using ERP.Web.API.Model;
using Newtonsoft.Json;

namespace ERP.Web.API.Controllers.Expedition
{
    [Route("ep-ap-report")]
    [ApiController]
    public class EpApReportController : ControllerBase
    {
        private readonly IEPAPReportService _epap;

        public EpApReportController(IEPAPReportService epap)
        {
            _epap = epap;
        }

        [HttpGet]
        public IActionResult GetData(int type, string supCode, string date, string sorts)
        {
            var result = _epap.GetData(type, date, supCode, JsonConvert.DeserializeObject<List<Sort>>(!string.IsNullOrWhiteSpace(sorts) ? sorts : "[]"));

            return Ok(new ApiResponse
            {
                RowCount = result.Total,
                TableData = result.Data.ToDynamicList()
            });
        }
    }
}
